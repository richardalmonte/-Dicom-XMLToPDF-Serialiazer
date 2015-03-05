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
    public class DicomDataElement
    {

        public string ElementTag { get; set; }        
        public string ElementVR { get; set; }
        public string TagDescription { get; set; }
        public long Length { get; set; }        
        public string ElementValue { get; set; }
        public List<string> FrameValue { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DicomDataElement()
        {
            ElementTag = string.Empty;
            TagDescription = string.Empty;
            ElementVR = string.Empty;
            ElementValue = string.Empty;
            FrameValue = new List<string>();
            Length = 0;
            
        }
        public void Dispose()
        {
            ElementTag = string.Empty;
            ElementVR = string.Empty;
            ElementValue = string.Empty;
            FrameValue.Clear();
            FrameValue = null;
            Length = 0;
        }

        /// <summary>
        /// Sets the element values.
        /// </summary>
        /// <param name="value">Byte array with the Element Value field</param>
        public void SetValue(byte[] value)
        {
            byte[] notToUse = { 0x01, 0x00 };
            byte[] notToUseRev = {0x00,0x01};
            string stringValue = ASCIIEncoding.ASCII.GetString(value);
            bool at = false; // ElementVR "Attribute Tag used for the parsing of tag "0028,0009".
            switch (ElementTag)
            {
                case "00181063":
                    DicomDecoder._FrameTime = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                case "00280002":
                    DicomObject.PixelSample = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280004":
                    DicomObject.PhotoInterpretation = ASCIIEncoding.UTF8.GetString(value).TrimEnd(' ');
                    DicomReader.IsInt = false;
                    break;
                case "00280005":
                case "00280200":
                    DicomReader.IsInt = true;
                    break;
                case "00280010":
                    DicomObject.PixelRows = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "60000010":
                    DicomObject.OverlayRows = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280011":
                    DicomObject.PixelColumns = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "60000011":
                    DicomObject.OverlayColumns = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280100":
                    DicomObject.BitsAllocated = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "60000100":
                    DicomObject.OverlayBitsAllocated = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280101":
                    DicomObject.BitStored = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280102":
                    DicomObject.HighBit = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00280103":
                    DicomObject.PixelRepresentation = BitConverter.ToInt16(value, 0);
                    DicomReader.IsInt = true;
                    break;
                case "00281050":
                    try
                    {
                        DicomObject.WindowCentre = Convert.ToDouble(stringValue, new CultureInfo("en-US")); 
                    }
                    catch (FormatException)
                    {
                        char[] split = { ' ', '\'','\\' };
                        string[] stVal = stringValue.Trim(' ').Split(split);
                        DicomObject.WindowCentre = Convert.ToDouble(stVal[0], new CultureInfo("en-US"));
                    }
                                      
                    break;
                case "00281051":
                    try
                    {
                        DicomObject.WindowWidth = Convert.ToDouble(stringValue, new CultureInfo("en-US"));   
                    }
                    catch (FormatException)
                    {

                        char[] split = { ' ', '\'', '\\' };
                        string[] stVal = stringValue.Trim(' ').Split(split);
                        DicomObject.WindowWidth = Convert.ToDouble(stVal[0], new CultureInfo("en-US"));
                    }
                                     
                    break;
                case "00281052":
                    DicomObject.RescaleIntercept = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                case "00281053":
                    DicomObject.RescaleSlope = Convert.ToDouble(stringValue, new CultureInfo("en-US"));
                    break;
                
                    
                case "00200013":
                    DicomObject.ImgNumber = Convert.ToInt32(DicomReader.GetString(value,false));
                    //DicomReader.IsInt = true;
                    break;
                case "00020010":
                    if (value[value.Length - 1] == 0x00)
                        Array.Resize<byte>(ref value, value.Length - 1);
                    DicomObject.TransferSyntax = ASCIIEncoding.UTF8.GetString(value);
                    switch (DicomObject.TransferSyntax)
                    {
                        case "1.2.840.10008.1.2":
                            DicomDecoder.Encode = FileEncode.ImplicitVR;
                            break;
                        case "1.2.840.10008.1.2.1":
                            DicomDecoder.Encode = FileEncode.ExplicitLittle;
                            break;
                        case "1.2.840.10008.1.2.2":
                            DicomDecoder.Encode = FileEncode.ExplicitBig;
                            DicomReader.IsLittleEndian = false;
                            break;
                        case "1.2.840.10008.1.2.4.70":
                        case"1.2.840.10008.1.2.4.80":
                        case "1.2.840.10008.1.2.4.91":
                            DicomDecoder.Encode = FileEncode.jpeg;
                            DicomReader.IsLittleEndian = false;
                            break;
                            //1.2.840.10008.1.2.2.4.70
                        default:
                            break;
                    }
                    break;
                case "00280008": // Number of Frames
                    DicomObject.NumOfFrames = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(value));
                    break;
                case "00280009": // Frame Increment Pointer
                    DicomReader.IsTag = true;
                    at = true;
                    break;
                // Palette Color Lookup Table Descriptor                
                case "00281101": 
                    DicomReader.IsLut = true;
                    DicomObject.RedPaletteLookupTable = DicomReader.GetLutValue(value);
                    break;
                //Green Palette Color Lookup Table Descriptor
                case "00281102":                     
                    DicomReader.IsLut = true;
                    DicomObject.GreenPaletteLookupTable = DicomReader.GetLutValue(value);
                    break;
                //Blue Palette Color Lookup Table Descriptor
                case "00281103":                   
                    DicomReader.IsLut = true;
                    DicomObject.BluePaletteLookupTable = DicomReader.GetLutValue(value);
                    break;
                case "00281201": //red palette
                    DicomReader.Reds = DicomReader.GetLut(value);
                    DicomReader.IsPalette = true;
                    break;
                case "00281202": // green palette
                    DicomReader.Greens = DicomReader.GetLut(value);
                    DicomReader.IsPalette = true;
                    break;
                case "00281203": // blue palette
                    DicomReader.Blues = DicomReader.GetLut(value);
                    DicomReader.IsPalette = true;
                    break; 
                case "60003000":
                    //Array.Resize<byte>(ref value, value.Length - 1);
                    //DicomDecoder.ImageFound = true;
                    if (DicomDecoder.InSequence)
                    {
                        DicomDecoder.IsImage = false;
                    }
                    else
                    {
                        DicomDecoder.IsImage = false;
                        DicomDecoder.ImageType = ImageType.Overlay;
                    }
                        
                    Array.Resize<byte>(ref value, 1);
                    value[0] = 0x00;
                    break;
                case "7FE00010":
                    DicomDecoder.ImageFound = true;
                    if(DicomDecoder.Encode == FileEncode.jpeg)
                    {
                        DicomDecoder.IsImage = true;
                        DicomDecoder.InSequence = false;
                    }
                    else if (DicomDecoder.InSequence)
                    {
                        DicomDecoder.IsImage = false;
                        //DicomDecoder.IsOW = true;
                    }
                    else
                        DicomDecoder.IsImage = true;
                    if (ElementVR == "OW")
                        DicomReader.IsBigEndian = true;
                    Array.Resize<byte>(ref value, 1);
                    value[0] = 0x00; 
                    break;
                case "FFFEE000":
                    if (value.Length < 1)
                        DicomDecoder.InSequence = true;
                    else
                    {
                       // DicomDecoder.InSequence = false;
                        if (DicomDecoder.ImageFound)
                        {
                            DicomDecoder.IsImage = true;
                            DicomDecoder.ImageType = ImageType.OriginalImage;
                        }
                    }
                    break;
                case "FFFEE0DD":
                    UTF8Encoding enco = new UTF8Encoding();
                    value = enco.GetBytes("(End of Sequence Data)");
                    DicomDecoder.InSequence = false;
                    break;
                default:

                    break;
            }
            if (value.SequenceEqual<byte>(notToUse) && !DicomReader.IsInt || value.SequenceEqual<byte>(notToUseRev) && !DicomReader.IsInt)
                ElementValue = "0";
            else if (ElementTag.Substring(4, 4) == "0000")
            {
                DicomReader.IsInt = true;
                ElementValue = DicomReader.GetString(value, false);
            }
            else if (DicomDecoder.InSequence && DicomDecoder.ImageFound == false)
            {
                ElementValue = "(Sequence Data)";
                DicomDecoder.InSequence = false;
                DicomReader.StartPosition = DicomReader.StartPosition - Length;
            }
            else if (DicomDecoder.IsImage)
            {
                ElementValue = "0";
                DicomReader.StartPosition = DicomReader.StartPosition - Length;
            }
            else if (DicomReader.IsPalette)
            {
                DicomReader.IsPalette = false;
                ElementValue = "0";
            }else if (ElementVR== "FD")
            {
                ElementValue = DicomReader.GetString(value, ElementVR);
            }
            else
                ElementValue = DicomReader.GetString(value, DicomReader.IsTag, at);

        }
    }
}
