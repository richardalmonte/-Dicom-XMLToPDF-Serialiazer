/*
 * This Software  and its supporting documentation were developed by 
 *      
 *      Richard Almonte
 *      
 *  As part of the project for internship at Sygest S.R.L, Parma Italy.
 *  Company's  tutor:  Stefano Maestri.  
 *  Academic Tutor: Federico Bergenti
 *  
 * 
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

using System.Text;

/// Using the FO-Dicom Library
using Dicom.IO;
using Dicom.IO.Reader;
using Dicom.Imaging;
using Dicom.Imaging.Algorithms;
using Dicom.Imaging.Codec;
using Dicom.Imaging.Codec.Jpeg;
using Dicom.Imaging.LUT;
using Dicom.Imaging.Mathematics;
using Dicom.Imaging.Render;
using Dicom.Media;
using Dicom;
//using CharLS;

namespace DicomXml
{
    class DicomDecoder
    {
        /// <summary>
        /// "Special" element VR.
        /// </summary>
        const string OB = "OB";
        const string OW = "OW";
        const string OF = "OF";
        const string SQ = "SQ";
        const string UN = "UN";
        const string UT = "UT";
        const string UL = "UL";
        const string FL = "FL";
        const string SS = "SS";
        const string US = "US";
        const string SL = "SL";
        const string DA = "DA";

        //const string ITEM = "FFFEE000";
        //const string ITEM_DELIMITATION = "FFFEE00D";
        //const string SEQUENCE_DELIMITATION = "FFFEE0DD";
        public static bool isOW { get; set; }
        public static bool isImage;
        public static bool inSequence { get; set; }
        public static FileEncode encode;
        public static ImageType imageType;
        DicomReader read;
        public DicomObject dicomObject;
        DicomDictionary dict;
        DicomDataElement elem;
        DicomImage img;
        public List<Bitmap> bmp;
        public static double _frameTime { get; set; }
        public double frameTime;
        public int frameWidth, frameHeight;
        Image Fo_image;
        string filepath;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Dicom file Path.</param>
        public DicomDecoder(string path)
        {
            filepath = path;

            read = new DicomReader(path);
            dicomObject = new DicomObject();
            encode = FileEncode.explicitLittle; // default value.
            isOW = false;
            _frameTime = 0;
        }

        /// <summary>
        /// Decode the entire file.
        /// </summary>
        public void processFile()
        {
            if (read.isDicom())
            {
                dict = new DicomDictionary();

                
                while (DicomReader.startPosition < DicomReader.EOF)
                {
                    elem = new DicomDataElement();
                    elem.elementTag = read.readTag();
                    elem.tagDescription = dict.dictLookup(elem.elementTag);
                    /*
                     * Verificare caso con immagini in sequenza. ovvero con element length = FFFFFFFF.
                     * */
                    if (elem.tagDescription != null)
                    {

                        switch (encode)
                        {
                            case FileEncode.implicitVR:
                                short count = 2;
                                if (elem.elementTag.Substring(0, 4) == "0002")
                                    elem.elementVR = DicomReader.getString(read.reader(2), false);
                                else
                                {
                                    elem.elementVR = dict.dictLookup(elem.elementTag, true);
                                    count = 4;
                                }
                                elem.length = read.getLength(count); //DicomReader.bytesToRead

                                break;
                            //end case implicit
                            case FileEncode.explicitLittle:
                            case FileEncode.explicitBig:
                            case FileEncode.jpeg:
                                if (elem.tagDescription.Equals("Item") || elem.tagDescription.Equals("ItemDelimitationItem"))
                                    elem.elementVR = "SEQ";
                                else if (elem.tagDescription.Equals("SequenceDelimitationItem"))
                                    elem.elementVR = "EOS" ; //End of Sequence.
                                else
                                {
                                    DicomReader.isInt = false;
                                    elem.elementVR = DicomReader.getString(read.reader(2), false);
                                }
                                switch (elem.elementVR)
                                {

                                    case OB:
                                    case OW:
                                    case OF:
                                    case UN:
                                    case UT:
                                        DicomReader.startPosition += 2;
                                        elem.length = read.getLength(4);
                                        //DicomReader.bytesToRead = elem.length;
                                        break;
                                    case UL:
                                        DicomReader.isInt = true;
                                        elem.length = read.getLength(2);
                                        //DicomReader.bytesToRead = elem.length;
                                        break;
                                    case SQ:
                                        DicomReader.startPosition += 2;
                                        elem.length = read.getLength(4);
                                        DicomDecoder.inSequence = true;
                                        break;
                                    case "SEQ":
                                        //DicomReader.startPosition += 4;
                                        elem.length = read.getLength(4); //0;
                                        DicomDecoder.inSequence = true;
                                        break;
                                    case "EOS":
                                        elem.length = 0;
                                        DicomReader.startPosition += 4;
                                        DicomDecoder.inSequence=false;
                                        break;
                                    case SS:
                                    case US:
                                    case SL:
                                        elem.length = read.getLength(2);
                                        DicomReader.isInt = true;
                                        break;
                                    case FL:
                                        elem.length = read.getLength(2);
                                        DicomReader.isFloat = true;
                                        break;
                                    case DA:
                                        elem.length = read.getLength(2);
                                        DicomReader.isDate = true;
                                        break;
                                    default:
                                        elem.length = read.getLength(2);
                                        //DicomReader.bytesToRead = elem.length;
                                        break;
                                }
                                if (elem.length == 0xFFFFFFFF || elem.length == -1 || elem.elementVR == SQ) //DicomReader.bytesToRead
                                {
                                    elem.length = 0; //DicomReader.bytesToRead              
                                    DicomReader.isLittleEndian = false;
                                    DicomDecoder.inSequence = true;
                                }
                                else
                                {
                                    DicomReader.isLittleEndian = true;
                                    //DicomDecoder.inSequence = false;
                                }
                                break;
                            default:
                                break;
                        }

                        elem.setValue(read.reader(elem.length));
                        if (isImage)// && elem.elementTag == "7FE00010")
                        {
                            elem.elementValue = "0"; // it can be the image path!!!
                            int i = 0;
                            img = new DicomImage();
                            bmp = new List<Bitmap>();
                            int bitAll;
                            int columns;
                            int rows;
                            int UncompressedFrameSize = img.setValues(imageType, out bitAll, out columns, out rows);
                            string imgPath = ConfigurationManager.AppSettings["ImageOutPath"];

                            if (encode == FileEncode.jpeg)
                            {
                                
                                //DicomJPEG pro = new DicomJPEG();
                                //read.readPixels(UncompressedFrameSize,bitAll,columns,rows);
                                read.closeReader();
                                
                                var file = DicomFile.Open(filepath);
                                Dicom.Imaging.DicomImage ima = new Dicom.Imaging.DicomImage(file.Dataset, 0);
                                Fo_image = ima.RenderImage();
                                elem.frameValue.Add(img.toBase64String((Bitmap)Fo_image));
                                bmp.Add(new Bitmap(columns, rows));
                                bmp[0] = (Bitmap)Fo_image;

                                DicomReader.startPosition = DicomReader.EOF + 1;
                               //bmp.Add(new Bitmap(columns, rows));
                               //bmp[i] =   read.readJPEG();
                                
                               // elem.frameValue.Add(img.toBase64String(bmp[i]));
                            }
                            else
                            {
                                while (i <= DicomObject.numOfFrames - 1)
                                {
                                    read.readPixels(UncompressedFrameSize, bitAll, columns, rows);

                                    Image myImage = new Bitmap(columns, rows);

                                    //myImage.Source = img.decodeImage();
                                    //myImage.Stretch = Stretch.None;
                                    //myImage.Margin = new Thickness(20);


                                    bmp.Add(new Bitmap(columns, rows));
                                    bmp[i] = img.createImage();
                                    img.SaveImage(imgPath + "newImage" + i + ".jpg"); // would be the value of the pixel tag!

                                    elem.frameValue.Add(img.toBase64String(bmp[i])); //the image file is coded into base64String.
                                    i++;
                                }
                            }

                            isImage = false;
                            imageFound = false;
                            overlayFound = false;
                            frameTime = _frameTime > 0 ? _frameTime : 0;
                        }
                        dicomObject.myDicomObject.Add(this.elem);
                        frameHeight = DicomObject.pixelRows;
                        frameWidth = DicomObject.pixelColumns;
                        
                    }
                }

            }
        }

        public void dispose()
        {
            this.elem.dispose();
            this.read.closeReader();
            this.dicomObject.dispose();
            if (DicomReader.pixels8 != null)
                DicomReader.pixels8 = null;
            if (DicomReader.pixels16 != null || DicomReader.pixels16Int != null)
            {
                DicomReader.pixels16 = null;
                DicomReader.pixels16Int = null;
            }
            if (DicomReader.pixels24 != null)
                DicomReader.pixels24 = null;

        }

        public static bool imageFound { get; set; }

        public static bool overlayFound { get; set; }
    }
}
                        
                