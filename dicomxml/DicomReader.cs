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
using System.Text;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using System.Drawing;

namespace DicomXml
{
    class DicomReader
    {
        public static string filePath = string.Empty;
        private FileStream myFileStreamer;
        //FileStream myFS;
        public static long startPosition { get; set; }
        public static long bytesToRead { get; set; }
        public static long EOF { get; set; }

        const String DICM = "DICM";
        public static bool isLittleEndian;

        public static bool isTag { get; set; }
        public static bool isInt { get; set; }
        public static byte[] pixelBuffer { get; set; }
        public static List<byte> pixels8 { get; set; }
        public static List<byte> pixels24 { get; set; } // 8 bits bit depth, 3 samples per pixel
        public static List<byte> pixels16 { get; set; }
        public static List<int> pixels16Int { get; set; }
        public static byte[] reds;
        public static byte[] greens;
        public static byte[] blues;
        int min8 = Byte.MinValue;
        int max8 = Byte.MaxValue;
        int min16 = short.MinValue;
        int max16 = ushort.MaxValue;
        public static bool isPalette { get; set; }
        public static bool isLut { get; set; }
        
        public DicomReader(string path)
        {
            filePath = path;
            try
            {
                myFileStreamer = new FileStream(path, FileMode.Open);
                
                EOF = myFileStreamer.Length;
            }
            catch (IOException)
            {
                MessageBox.Show("Error: The file you are trying to open is being used by another proccess.", "Cannot access the file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            isLittleEndian = true;
        }
        public DicomReader()
        {
            myFileStreamer.Close();
        }

        /// <summary>
        /// Read from the file the specified "toRead" bytes number at the "startPos" position.
        /// </summary>
        /// <param name="startPos">Position to begin to read</param>
        /// <param name="toRead">Bytes to read</param>
        /// <returns>read bytes as buffer.</returns>
        public byte[] reader(long startPos, long toRead)
        {
            byte[] buffer = new byte[toRead];
            myFileStreamer.Position = startPos;
            
            int i = 0;
            bytesToRead = toRead;
            while (i < toRead)
            {
                buffer[i] = (byte)myFileStreamer.ReadByte();
                ++i;
            }
            startPosition = myFileStreamer.Position;
            return buffer;
        }
        
        //public  Bitmap readJPEG()
        //{
            
            
        //   // Stream st = new MemoryStream(myFileStreamer.Read(stArray, (int)myFileStreamer.Position, (int)bytesToRead));
        //    DicomObjects.DicomCodecs.CodecGlobal.RegisterCodec(new JPEGLSCodec.LSFactory());
        //    //Stream fs = new FileStream(filePath, FileMode.Open);
            
        //    DicomObjects.DicomImage im = new DicomObjects.DicomImage(filePath);
        //    im.Copy();
        //    return im.Bitmap();
        //}
        public byte[] reader(long toRead)
        {
            return reader(startPosition, toRead);
        }

        public static int[] getLutValue(byte[] val)
        {
            int[] lut = new int[3];
            lut[0]= Convert.ToInt32(val[0]);
            lut[1] = Convert.ToInt32(val[2]);
            lut[2] = Convert.ToInt32(val[4]);
            return lut;
        }
        /// <summary>
        /// Convert a Byte array to an string.
        /// </summary>
        /// <param name="toString">the byte array to be converted</param>
        /// <param name="isTag">if is an Element tag  must be swapped else can be converted withou any operation</param>
        /// <param name="isAT">elementVR "Attribute Tag used for the parsing of tag "0028,0009".</param>
        /// <returns>Returns the string</returns>
        public static String getString(byte[] toString, bool isTag, bool isAT)
        {
            StringBuilder sb = new StringBuilder();
            int length = 0;
            if (toString.Length == 0 || toString.Length> 500)
                return "0";
            if (!isTag)
            {
                if (isInt)
                {
                    isInt = false;
                    if (toString.Length == 2)
                    {

                        length = BitConverter.ToInt16(toString, 0);
                    }
                    else
                    {
                        length = BitConverter.ToInt32(toString, 0);
                    }
                    return length.ToString();
                }
                else if (isLut)
                {
                    isLut = false;
                    StringBuilder sbuild = new StringBuilder();                    
                    var lut = DicomReader.getLutValue(toString);
                    sb.Append(lut[0].ToString());
                    sb.Append(" \\");
                    sb.Append(lut[1].ToString());
                    sb.Append(" \\");
                    sb.Append(lut[2].ToString());
                    
                    return sb.ToString().TrimEnd(' ');
                }
                else if(isFloat)
                {
                    isFloat = false;
                    double val = BitConverter.ToSingle(toString, 0);

                    return val.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    
                }
                else if (isDate)
                {
                    isDate = false;
                    return ASCIIEncoding.UTF8.GetString(toString).Trim('.').TrimEnd(Convert.ToChar(0x00), ' ');
                }
                else
                    return ASCIIEncoding.UTF8.GetString(toString).TrimEnd(Convert.ToChar(0x00), ' ');
            }
            else
            {
                DicomReader.isTag = false;
                byte[] groupTag = new byte[2];
                byte[] elemTag = new byte[2];

                for (int i = 0; i < 4; i++)
                {
                    if (i < 2)
                    {
                        groupTag[i] = toString[i];
                    }
                    else if (i >= 2 && i < 4)
                    {
                        elemTag[i - 2] = toString[i];
                    }
                }
                Array.Reverse(groupTag);
                Array.Reverse(elemTag);

                foreach (byte item in groupTag)
                {
                    sb.Append(item.ToString("X2"));
                }
                if (isAT)
                    sb.Append(',');
                foreach (byte i in elemTag)
                {
                    sb.Append(i.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static String getString(byte[] toString, string VR)
        {
            switch (VR) 
            {
                case "FD":
                    Double val = BitConverter.ToDouble(toString, 0);
                    return val.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));                    
                default:
                    return ASCIIEncoding.UTF8.GetString(toString).TrimEnd(Convert.ToChar(0x00), ' ');
            }
        }
        public static String getString(byte[] toString, bool isTag)
        {
          return  getString(toString, isTag, false);
        }
        public string readTag()
        {
            byte[] buffer = new byte[4];
            buffer = reader(startPosition, 4);

            return getString(buffer, true);
        }
        /// <summary>
        /// Verify if the current file is a Dicom file. 
        /// Check if exist the Dicom Standard header info (128 first bits = 0) and get the DICM header
        /// else verify if is a Dicom file without the Dicom 3 Standard form.
        /// </summary>
        /// <returns> true/false if is/not DICOM</returns>
        public bool isDicom()
        {
            byte[] buff = reader(128, 4);

            if (getString(buff, false) == DICM)
                return true;
            else
            {
                buff = reader(0, 4);
                DicomReader.startPosition = 0;
                DicomDecoder.encode = FileEncode.implicitVR;
                DicomDictionary dict = new DicomDictionary();
                if (dict.dictLookup(getString(buff, true)) != null)
                    return true;
                else
                    return false;
            }

        }
        /// <summary>
        /// Get the length of the current element value field.
        /// </summary>
        /// <param name="count">Number of byte to be read</param>
        /// <returns>The number of bytes to read as long int</returns>
        public long getLength(short count)
        {
            int length = 0;
            byte[] buffer = new byte[count];
            buffer = reader(startPosition, count);
            
            if (count == 2)
            {
                length = BitConverter.ToInt16(buffer, 0);
            }
            else
            {
                length = BitConverter.ToInt32(buffer, 0);
            }
            
            return length;
        }
        /// <summary>
        /// Read the image pixels from the pixel data Item tag (7FE00010)
        /// </summary>
        public void readPixels(int toRead)
        {
           
            if (DicomObject.pixelSample == 1 && DicomObject.bitsAllocated == 8)
            {
                if (pixels8 != null)
                    pixels8.Clear();
                pixels8 = new List<byte>();
                int numPixels = DicomObject.pixelColumns * DicomObject.pixelRows;
                byte[] buff = new byte[numPixels*2];

                buff = reader(startPosition, toRead);
                pixelBuffer = buff;
                for (int i = 0; i < numPixels; ++i)
                {
                    int pixVal = (int)(buff[i] * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                    if (DicomObject.photoInterpretation == "MONOCHROME1")
                        pixVal = max8 - pixVal;
                    pixels8.Add((byte)(DicomObject.pixelRepresentation == 1 ? pixVal : (pixVal - min8)));
                }
                
            }
            if(DicomObject.pixelSample==1&& DicomObject.bitsAllocated==16)
            {
                if(pixels16!=null)
                    pixels16.Clear();
                if(pixels16Int!=null)
                    pixels16Int.Clear();
                pixels16 = new List<byte>();
                pixels16Int= new List<int>();

                int numPixels = DicomObject.pixelColumns * DicomObject.pixelRows;

                byte[] bufByte = new byte[numPixels*2];
                byte[] signedData = new byte[2];
                bufByte= reader(startPosition, numPixels*2);
                ushort unsignedS;
                int i, i1, pixVal;
                byte b0, b1;
                pixelBuffer = bufByte;
                for (i = 0; i < numPixels; ++i)
                {
                    i1 = i * 2; b0 = bufByte[i1];
                    b1 = bufByte[i1 + 1];
                    unsignedS = Convert.ToUInt16((b1 << 8) + b0);
                    if (DicomObject.pixelRepresentation == 0)
                    {
                        pixVal = (int)(unsignedS * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                        if (DicomObject.photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    else
                    {
                        signedData[0] = b0;
                        signedData[1] = b1;
                        short sVal = BitConverter.ToInt16(signedData, 0);

                        pixVal = (int)(sVal * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                        if (DicomObject.photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    pixels16Int.Add(pixVal);
                }
                int minPixVal = pixels16Int.Min();
                DicomObject.signedImage = false;
                if (minPixVal < 0)
                    DicomObject.signedImage = true;

                foreach (int pixel in pixels16Int)
                {
                    if (DicomObject.signedImage)
                        pixels16.Add((byte)(pixel - min16));
                    else
                        pixels16.Add((byte)(pixel));
                }
                pixels16Int.Clear();
                
            }

            if (DicomObject.pixelSample == 3 && DicomObject.bitsAllocated == 8)
            {
                DicomObject.signedImage = false;
                if (pixels24 != null)
                    pixels24.Clear();
                pixels24 = new List<byte>();
                int numPixels = DicomObject.pixelColumns * DicomObject.pixelRows;
                int numBytes = numPixels * DicomObject.pixelSample;
                byte[] myBuf = new byte[numBytes];
                myBuf = reader(startPosition, numBytes);
                
                pixelBuffer = myBuf;

                for (int i = 0; i < numBytes; ++i)
                {
                    pixels24.Add(myBuf[i]);
                }
            }
            
        }
        public void readPixels(int toRead, int bAll, int columns, int rows)
        {
            if (DicomObject.pixelSample == 1 && bAll == 8)
            {
                if (pixels8 != null)
                    pixels8.Clear();
                pixels8 = new List<byte>();
                int numPixels = columns * rows;
                byte[] buff = new byte[numPixels * 2];
                
                buff = reader(startPosition, toRead);
                pixelBuffer = buff;

                for (int i = 0; i < numPixels; ++i)//numPixels
                {
                    int pixVal = (int)(buff[i] * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                    if (DicomObject.photoInterpretation == "MONOCHROME1")
                        pixVal = max8 - pixVal;
                    pixels8.Add((byte)(DicomObject.pixelRepresentation == 1 ? pixVal : (pixVal - min8)));
                    DicomImage.imgPixel[i] = pixels8[i];
                }
                
            }
            if (DicomObject.pixelSample == 1 && bAll == 16)
            {
                if (pixels16 != null)
                    pixels16.Clear();
                if (pixels16Int != null)
                    pixels16Int.Clear();
                pixels16 = new List<byte>();
                pixels16Int = new List<int>();

                int numPixels = columns * rows;

                byte[] bufByte = new byte[numPixels * 2];
                byte[] signedData = new byte[2];
                bufByte = reader(startPosition, numPixels * 2);
                pixelBuffer = bufByte;

                ushort unsignedS;
                int i, i1, pixVal;
                byte b0, b1;

                for (i = 0; i < numPixels; ++i)
                {
                    i1 = i * 2; b0 = bufByte[i1];
                    b1 = bufByte[i1 + 1];
                    unsignedS = Convert.ToUInt16((b1 << 8) + b0);
                    if (DicomObject.pixelRepresentation == 0)
                    {
                        pixVal = (int)(unsignedS * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                        if (DicomObject.photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    else
                    {
                        signedData[0] = b0;
                        signedData[1] = b1;
                        short sVal = BitConverter.ToInt16(signedData, 0);

                        pixVal = (int)(sVal * DicomObject.rescaleSlope + DicomObject.rescaleIntercept);
                        if (DicomObject.photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    pixels16Int.Add(pixVal);
                    
                }
                int minPixVal = pixels16Int.Min();
                DicomObject.signedImage = false;
                if (minPixVal < 0)
                    DicomObject.signedImage = true;
                int j = 0;
                DicomImage.imgPixel = new byte[pixels16Int.Count];
                foreach (int pixel in pixels16Int)
                {
                    if (DicomObject.signedImage)
                        pixels16.Add((byte)(pixel - min16));
                    else
                        pixels16.Add((byte)(pixel));
                    DicomImage.imgPixel[j] = (byte)pixel;
                    j++;
                }
                pixels16Int.Clear();

            }

            if (DicomObject.pixelSample == 3 && bAll == 8)
            {
                DicomObject.signedImage = false;
                if (pixels24 != null)
                    pixels24.Clear();
                pixels24 = new List<byte>();
                int numPixels = columns* rows;
                int numBytes = numPixels * DicomObject.pixelSample;
                byte[] myBuf = new byte[numBytes];
                myBuf = reader(startPosition, numBytes);
                pixelBuffer = myBuf;

                for (int i = 0; i < numBytes; ++i)
                {
                    pixels24.Add(myBuf[i]);
                }
            }

        }

       
        public static byte[] getLut(byte[] value)
        {
            int length = value.Length;
            if ((length & 1) != 0) //odd
                return null;
            length /= 2;
            byte[] lut = new byte[length];
            for (int i = 0; i < length; ++i)
                lut[i] = Convert.ToByte(value[i] >> 8);
            return lut;
        }
        /// <summary>
        /// Close the FileStream.
        /// </summary>
        /// 
        public void closeReader()
        {
            this.myFileStreamer.Close();
            this.myFileStreamer.Dispose();
        }



        public static bool isFloat { get; set; }

        public static bool isBigEndian { get; set; }

        public static bool isDate { get; set; }
    }
}
