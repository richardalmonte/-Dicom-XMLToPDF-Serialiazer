/*
 * This Software  and its supporting documantation were developed by 
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
using System.Globalization;
using System.Linq;
using System.Text;

namespace DicomXml
{
    class DicomDataElement
    {

        public string elementTag { get; set; }        
        public string elementVR { get; set; }
        public string tagDescription { get; set; }
        public long length { get; set; }        
        public string elementValue { get; set; }
        public List<string> frameValue { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DicomDataElement()
        {
            elementTag = string.Empty;
            tagDescription = string.Empty;
            elementVR = string.Empty;
            elementValue = string.Empty;
            frameValue = new List<string>();
            length = 0;
            
        }
        public void dispose()
        {
            elementTag = string.Empty;
            elementVR = string.Empty;
            elementValue = string.Empty;
            frameValue.Clear();
            frameValue = null;
            length = 0;
        }

        /// <summary>
        /// Sets the element values.
        /// </summary>
        /// <param name="value">Byte array with the Element Value field</param>
        public void setValue(byte[] value)
        {
            byte[] notToUse = { 0x01, 0x00 };
            byte[] notToUseRev = {0x00,0x01};
            string stringValue = ASCIIEncoding.ASCII.GetString(value);
            bool AT = false; // elementVR "Attribute Tag used for the parsing of tag "0028,0009".
            switch (elementTag)
            {
                case "00181063":
                    DicomDecoder._frameTime = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                case "00280002":
                    DicomObject.pixelSample = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280004":
                    DicomObject.photoInterpretation = ASCIIEncoding.UTF8.GetString(value).TrimEnd(' ');
                    DicomReader.isInt = false;
                    break;
                case "00280005":
                case "00280200":
                    DicomReader.isInt = true;
                    break;
                case "00280010":
                    DicomObject.pixelRows = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "60000010":
                    DicomObject.OverlayRows = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280011":
                    DicomObject.pixelColumns = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "60000011":
                    DicomObject.OverlayColumns = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280100":
                    DicomObject.bitsAllocated = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "60000100":
                    DicomObject.OverlayBitsAllocated = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280101":
                    DicomObject.bitStored = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280102":
                    DicomObject.highBit = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00280103":
                    DicomObject.pixelRepresentation = BitConverter.ToInt16(value, 0);
                    DicomReader.isInt = true;
                    break;
                case "00281050":
                    try
                    {
                        DicomObject.windowCentre = Convert.ToDouble(stringValue, new CultureInfo("en-US")); 
                    }
                    catch (FormatException)
                    {
                        char[] split = { ' ', '\'','\\' };
                        string[] stVal = stringValue.Trim(' ').Split(split);
                        DicomObject.windowCentre = Convert.ToDouble(stVal[0], new CultureInfo("en-US"));
                    }
                                      
                    break;
                case "00281051":
                    try
                    {
                        DicomObject.windowWidth = Convert.ToDouble(stringValue, new CultureInfo("en-US"));   
                    }
                    catch (FormatException)
                    {

                        char[] split = { ' ', '\'', '\\' };
                        string[] stVal = stringValue.Trim(' ').Split(split);
                        DicomObject.windowWidth = Convert.ToDouble(stVal[0], new CultureInfo("en-US"));
                    }
                                     
                    break;
                case "00281052":
                    DicomObject.rescaleIntercept = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                case "00281053":
                    DicomObject.rescaleSlope = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                
                    
                case "00200013":
                    DicomObject.imgNumber = Convert.ToInt32(DicomReader.getString(value,false));
                    //DicomReader.isInt = true;
                    break;
                case "00020010":
                    if (value[value.Length - 1] == 0x00)
                        Array.Resize<byte>(ref value, value.Length - 1);
                    DicomObject.transferSyntax = ASCIIEncoding.UTF8.GetString(value);
                    switch (DicomObject.transferSyntax)
                    {
                        case "1.2.840.10008.1.2":
                            DicomDecoder.encode = FileEncode.implicitVR;
                            break;
                        case "1.2.840.10008.1.2.1":
                            DicomDecoder.encode = FileEncode.explicitLittle;
                            break;
                        case "1.2.840.10008.1.2.2":
                            DicomDecoder.encode = FileEncode.explicitBig;
                            DicomReader.isLittleEndian = false;
                            break;
                        case "1.2.840.10008.1.2.4.70":
                        case"1.2.840.10008.1.2.4.80":
                        case "1.2.840.10008.1.2.4.91":
                            DicomDecoder.encode = FileEncode.jpeg;
                            DicomReader.isLittleEndian = false;
                            break;
                            //1.2.840.10008.1.2.2.4.70
                        default:
                            break;
                    }
                    break;
                case "00280008": // Number of Frames
                    DicomObject.numOfFrames = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(value));
                    break;
                case "00280009": // Frame Increment Pointer
                    DicomReader.isTag = true;
                    AT = true;
                    break;
                // Palette Color Lookup Table Descriptor                
                case "00281101": 
                    DicomReader.isLut = true;
                    DicomObject.RedPaletteLookupTable = DicomReader.getLutValue(value);
                    break;
                //Green Palette Color Lookup Table Descriptor
                case "00281102":                     
                    DicomReader.isLut = true;
                    DicomObject.GreenPaletteLookupTable = DicomReader.getLutValue(value);
                    break;
                //Blue Palette Color Lookup Table Descriptor
                case "00281103":                   
                    DicomReader.isLut = true;
                    DicomObject.BluePaletteLookupTable = DicomReader.getLutValue(value);
                    break;
                case "00281201": //red palette
                    DicomReader.reds = DicomReader.getLut(value);
                    DicomReader.isPalette = true;
                    break;
                case "00281202": // green palette
                    DicomReader.greens = DicomReader.getLut(value);
                    DicomReader.isPalette = true;
                    break;
                case "00281203": // blue palette
                    DicomReader.blues = DicomReader.getLut(value);
                    DicomReader.isPalette = true;
                    break; 
                case "60003000":
                    //Array.Resize<byte>(ref value, value.Length - 1);
                    //DicomDecoder.imageFound = true;
                    if (DicomDecoder.inSequence)
                    {
                        DicomDecoder.isImage = false;
                    }
                    else
                    {
                        DicomDecoder.isImage = false;
                        DicomDecoder.imageType = ImageType.Overlay;
                    }
                        
                    Array.Resize<byte>(ref value, 1);
                    value[0] = 0x00;
                    break;
                case "7FE00010":
                    DicomDecoder.imageFound = true;
                    if(DicomDecoder.encode == FileEncode.jpeg)
                    {
                        DicomDecoder.isImage = true;
                        DicomDecoder.inSequence = false;
                    }
                    else if (DicomDecoder.inSequence)
                    {
                        DicomDecoder.isImage = false;
                        //DicomDecoder.isOW = true;
                    }
                    else
                        DicomDecoder.isImage = true;
                    if (elementVR == "OW")
                        DicomReader.isBigEndian = true;
                    Array.Resize<byte>(ref value, 1);
                    value[0] = 0x00; 
                    break;
                case "FFFEE000":
                    if (value.Length < 1)
                        DicomDecoder.inSequence = true;
                    else
                    {
                       // DicomDecoder.inSequence = false;
                        if (DicomDecoder.imageFound)
                        {
                            DicomDecoder.isImage = true;
                            DicomDecoder.imageType = ImageType.OriginalImage;
                        }
                    }
                    break;
                case "FFFEE0DD":
                    UTF8Encoding enco = new UTF8Encoding();
                    value = enco.GetBytes("(End of Sequence Data)");
                    DicomDecoder.inSequence = false;
                    break;
                default:

                    break;
            }
            if (value.SequenceEqual<byte>(notToUse) && !DicomReader.isInt || value.SequenceEqual<byte>(notToUseRev) && !DicomReader.isInt)
                elementValue = "0";
            else if (elementTag.Substring(4, 4) == "0000")
            {
                DicomReader.isInt = true;
                elementValue = DicomReader.getString(value, false);
            }
            else if (DicomDecoder.inSequence && DicomDecoder.imageFound == false)
            {
                elementValue = "(Sequence Data)";
                DicomDecoder.inSequence = false;
                DicomReader.startPosition = DicomReader.startPosition - length;
            }
            else if (DicomDecoder.isImage)
            {
                elementValue = "0";
                DicomReader.startPosition = DicomReader.startPosition - length;
            }
            else if (DicomReader.isPalette)
            {
                DicomReader.isPalette = false;
                elementValue = "0";
            }else if (elementVR== "FD")
            {
                elementValue = DicomReader.getString(value, elementVR);
            }
            else
                elementValue = DicomReader.getString(value, DicomReader.isTag, AT);

        }
    }
}
