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

/// Using the FO-Dicom Library
using Dicom;
using System.IO;

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
        public static bool IsOW { get; set; }
        public static bool IsImage;
        public static bool InSequence { get; set; }
        public static FileEncode Encode;
        public static ImageType ImageType;
        readonly DicomReader read;
        public DicomObject DicomObject;
        DicomDictionary dict;
        DicomDataElement elem;
        DicomImage img;
        public List<Bitmap> Bmp;
        public static double _FrameTime { get; set; }
        public double FrameTime;
        public int FrameWidth, FrameHeight;
        Image foImage;
        readonly string filepath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Dicom file Path.</param>
        public DicomDecoder(string path)
        {
            filepath = path;

            read = new DicomReader(path);
            DicomObject = new DicomObject();
            Encode = FileEncode.ExplicitLittle; // default value.
            IsOW = false;
            _FrameTime = 0;
        }

        /// <summary>
        /// Decode the entire file.
        /// </summary>
        public void ProcessFile()
        {
            if (read.IsDicom())
            {
                dict = new DicomDictionary();

                
                while (DicomReader.StartPosition < DicomReader.EOF)
                {
                    elem = new DicomDataElement();
                    elem.ElementTag = read.ReadTag();
                    elem.TagDescription = dict.DictLookup(elem.ElementTag);
                    /*
                     * Verificare caso con immagini in sequenza. ovvero con element Length = FFFFFFFF.
                     * */
                    if (elem.TagDescription != null)
                    {

                        switch (Encode)
                        {
                            case FileEncode.ImplicitVR:
                                short count = 2;
                                if (elem.ElementTag.Substring(0, 4) == "0002")
                                    elem.ElementVR = DicomReader.GetString(read.Reader(2), false);
                                else
                                {
                                    elem.ElementVR = dict.DictLookup(elem.ElementTag, true);
                                    count = 4;
                                }
                                elem.Length = read.GetLength(count); //DicomReader.BytesToRead

                                break;
                            //end case implicit
                            case FileEncode.ExplicitLittle:
                            case FileEncode.ExplicitBig:
                            case FileEncode.jpeg:
                                if (elem.TagDescription.Equals("Item") || elem.TagDescription.Equals("ItemDelimitationItem"))
                                    elem.ElementVR = "SEQ";
                                else if (elem.TagDescription.Equals("SequenceDelimitationItem"))
                                    elem.ElementVR = "EOS" ; //End of Sequence.
                                else
                                {
                                    DicomReader.IsInt = false;
                                    elem.ElementVR = DicomReader.GetString(read.Reader(2), false);
                                }
                                switch (elem.ElementVR)
                                {

                                    case OB:
                                    case OW:
                                    case OF:
                                    case UN:
                                    case UT:
                                        DicomReader.StartPosition += 2;
                                        elem.Length = read.GetLength(4);
                                        //DicomReader.BytesToRead = elem.Length;
                                        break;
                                    case UL:
                                        DicomReader.IsInt = true;
                                        elem.Length = read.GetLength(2);
                                        //DicomReader.BytesToRead = elem.Length;
                                        break;
                                    case SQ:
                                        DicomReader.StartPosition += 2;
                                        elem.Length = read.GetLength(4);
                                        DicomDecoder.InSequence = true;
                                        break;
                                    case "SEQ":
                                        //DicomReader.StartPosition += 4;
                                        elem.Length = read.GetLength(4); //0;
                                        DicomDecoder.InSequence = true;
                                        break;
                                    case "EOS":
                                        elem.Length = 0;
                                        DicomReader.StartPosition += 4;
                                        DicomDecoder.InSequence=false;
                                        break;
                                    case SS:
                                    case US:
                                    case SL:
                                        elem.Length = read.GetLength(2);
                                        DicomReader.IsInt = true;
                                        break;
                                    case FL:
                                        elem.Length = read.GetLength(2);
                                        DicomReader.IsFloat = true;
                                        break;
                                    case DA:
                                        elem.Length = read.GetLength(2);
                                        DicomReader.IsDate = true;
                                        break;
                                    default:
                                        elem.Length = read.GetLength(2);
                                        //DicomReader.BytesToRead = elem.Length;
                                        break;
                                }
                                if (elem.Length == 0xFFFFFFFF || elem.Length == -1 || elem.ElementVR == SQ) //DicomReader.BytesToRead
                                {
                                    elem.Length = 0; //DicomReader.BytesToRead              
                                    DicomReader.IsLittleEndian = false;
                                    DicomDecoder.InSequence = true;
                                }
                                else
                                {
                                    DicomReader.IsLittleEndian = true;
                                    //DicomDecoder.InSequence = false;
                                }
                                break;
                            default:
                                break;
                        }

                        elem.SetValue(read.Reader(elem.Length));
                        if (IsImage)// && elem.ElementTag == "7FE00010")
                        {
                            elem.ElementValue = "0"; // it can be the image path!!!
                            int i = 0;
                            img = new DicomImage();
                            Bmp = new List<Bitmap>();
                            int bitAll;
                            int columns;
                            int rows;
                            int uncompressedFrameSize = img.SetValues(ImageType, out bitAll, out columns, out rows);
                            string imgPath = ConfigurationManager.AppSettings["ImageOutPath"];

                            if (Encode == FileEncode.jpeg)
                            {
                                
                                //DicomJPEG pro = new DicomJPEG();
                                //read.ReadPixels(UncompressedFrameSize,bitAll,columns,rows);
                                read.CloseReader();
                                
                                var file = DicomFile.Open(filepath);
                               
                                Dicom.Imaging.DicomImage ima = new Dicom.Imaging.DicomImage(file.Dataset, 0);
                                foImage = ima.RenderImage();
                                elem.FrameValue.Add(img.ToBase64String((Bitmap)foImage));
                                Bmp.Add(new Bitmap(columns, rows));
                                Bmp[0] = (Bitmap)foImage;

                                DicomReader.StartPosition = DicomReader.EOF + 1;
                               //Bmp.Add(new Bitmap(columns, rows));
                               //Bmp[i] =   read.readJPEG();
                                
                               // elem.FrameValue.Add(img.ToBase64String(Bmp[i]));
                            }
                            else
                            {
                                Stream file = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                Dicom.Imaging.DicomImage im = new Dicom.Imaging.DicomImage(DicomFile.Open(file).Dataset, 0);

                                while (i <= DicomObject.NumOfFrames - 1)
                                {
                                    read.ReadPixels(uncompressedFrameSize, bitAll, columns, rows);

                                    //Image myImage = new Bitmap(columns, rows);

                                    //myImage.Source = img.decodeImage();
                                    //myImage.Stretch = Stretch.None;
                                    //myImage.Margin = new Thickness(20);

                                    Bitmap image = new Bitmap(im.RenderImage(i));
                                        

                                    Bmp.Add(image);
                                    Bmp[i] = image;
                                    image.Save(imgPath + "newImage" + i + ".jpg"); // would be the value of the pixel tag!

                                    elem.FrameValue.Add(img.ToBase64String(Bmp[i])); //the image file is coded into base64String.
                                    i++;
                                }
                            }

                            IsImage = false;
                            ImageFound = false;
                            OverlayFound = false;
                            FrameTime = _FrameTime > 0 ? _FrameTime : 0;
                        }
                        DicomObject.MyDicomObject.Add(this.elem);
                        FrameHeight = DicomObject.PixelRows;
                        FrameWidth = DicomObject.PixelColumns;
                        
                    }
                }

            }
        }

        public void Dispose()
        {
            this.elem.Dispose();
            this.read.CloseReader();
            this.DicomObject.Dispose();
            if (DicomReader.Pixels8 != null)
                DicomReader.Pixels8 = null;
            if (DicomReader.Pixels16 != null || DicomReader.Pixels16Int != null)
            {
                DicomReader.Pixels16 = null;
                DicomReader.Pixels16Int = null;
            }
            if (DicomReader.Pixels24 != null)
                DicomReader.Pixels24 = null;

        }

        public static bool ImageFound { get; set; }

        public static bool OverlayFound { get; set; }
    }
}
                        
                