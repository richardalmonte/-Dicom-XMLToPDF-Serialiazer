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

namespace DicomXml
{
    class DicomReader
    {
        public static string FilePath = string.Empty;
        private readonly FileStream myFileStreamer;

        //FileStream myFS;
        public static long StartPosition { get; set; }
        public static long BytesToRead { get; set; }
        public static long EOF { get; set; }

        const String DICM = "DICM";
        public static bool IsLittleEndian;

        public static bool IsTag { get; set; }
        public static bool IsInt { get; set; }
        public static byte[] PixelBuffer { get; set; }
        public static List<byte> Pixels8 { get; set; }
        public static List<byte> Pixels24 { get; set; } // 8 bits bit depth, 3 samples per pixel
        public static List<byte> Pixels16 { get; set; }
        public static List<int> Pixels16Int { get; set; }
        public static byte[] Reds;
        public static byte[] Greens;
        public static byte[] Blues;
        readonly int min8 = Byte.MinValue;
        readonly int max8 = Byte.MaxValue;
        readonly int min16 = short.MinValue;
        readonly int max16 = ushort.MaxValue;

        public static bool IsPalette { get; set; }
        public static bool IsLut { get; set; }

        public DicomReader(string path)
        {
            FilePath = path;
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
            IsLittleEndian = true;
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
        public byte[] Reader(long startPos, long toRead)
        {

            try
            {
                byte[] buffer = new byte[toRead];
                myFileStreamer.Position = startPos;

                int i = 0;
                BytesToRead = toRead;
                while (i < toRead)
                {
                    buffer[i] = (byte)myFileStreamer.ReadByte();
                    ++i;
                }
                StartPosition = myFileStreamer.Position;
                return buffer;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);

            }

            return new byte[0];
        }

        //public  Bitmap readJPEG()
        //{


        //   // Stream st = new MemoryStream(myFileStreamer.Read(stArray, (int)myFileStreamer.Position, (int)BytesToRead));
        //    DicomObjects.DicomCodecs.CodecGlobal.RegisterCodec(new JPEGLSCodec.LSFactory());
        //    //Stream fs = new FileStream(FilePath, FileMode.Open);

        //    DicomObjects.DicomImage im = new DicomObjects.DicomImage(FilePath);
        //    im.Copy();
        //    return im.Bitmap();
        //}
        public byte[] Reader(long toRead)
        {
            return Reader(StartPosition, toRead);
        }

        public static int[] GetLutValue(byte[] val)
        {
            int[] lut = new int[3];
            lut[0] = Convert.ToInt32(val[0]);
            lut[1] = Convert.ToInt32(val[2]);
            lut[2] = Convert.ToInt32(val[4]);
            return lut;
        }
        /// <summary>
        /// Convert a Byte array to an string.
        /// </summary>
        /// <param name="toString">the byte array to be converted</param>
        /// <param name="IsTag">if is an Element tag  must be swapped else can be converted withou any operation</param>
        /// <param name="isAT">ElementVR "Attribute Tag used for the parsing of tag "0028,0009".</param>
        /// <returns>Returns the string</returns>
        public static String GetString(byte[] toString, bool isTag, bool isAT)
        {
            StringBuilder sb = new StringBuilder();
            int length = 0;
            if (toString.Length == 0 || toString.Length > 500)
                return "0";
            if (!isTag)
            {
                if (IsInt)
                {
                    IsInt = false;
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
                else if (IsLut)
                {
                    IsLut = false;
                    //StringBuilder sbuild = new StringBuilder();                    
                    var lut = DicomReader.GetLutValue(toString);
                    sb.Append(lut[0].ToString());
                    sb.Append(" \\");
                    sb.Append(lut[1].ToString());
                    sb.Append(" \\");
                    sb.Append(lut[2].ToString());

                    return sb.ToString().TrimEnd(' ');
                }
                else if (IsFloat)
                {
                    IsFloat = false;
                    double val = BitConverter.ToSingle(toString, 0);

                    return val.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

                }
                else if (IsDate)
                {
                    IsDate = false;
                    return ASCIIEncoding.UTF8.GetString(toString).Trim('.').TrimEnd(Convert.ToChar(0x00), ' ');
                }
                else
                    return ASCIIEncoding.UTF8.GetString(toString).TrimEnd(Convert.ToChar(0x00), ' ');
            }
            else
            {
                DicomReader.IsTag = false;
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

        public static String GetString(byte[] toString, string vr)
        {
            switch (vr)
            {
                case "FD":
                    Double val = BitConverter.ToDouble(toString, 0);
                    return val.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                default:
                    return ASCIIEncoding.UTF8.GetString(toString).TrimEnd(Convert.ToChar(0x00), ' ');
            }
        }
        public static String GetString(byte[] toString, bool isTag)
        {
            return GetString(toString, isTag, false);
        }
        public string ReadTag()
        {
            byte[] buffer = new byte[4];
            buffer = Reader(StartPosition, 4);

            return GetString(buffer, true);
        }
        /// <summary>
        /// Verify if the current file is a Dicom file. 
        /// Check if exist the Dicom Standard header info (128 first bits = 0) and get the DICM header
        /// else verify if is a Dicom file without the Dicom 3 Standard form.
        /// </summary>
        /// <returns> true/false if is/not DICOM</returns>
        public bool IsDicom()
        {
            byte[] buff = Reader(128, 4);

            if (GetString(buff, false) == DICM)
                return true;
            else
            {
                buff = Reader(0, 4);
                DicomReader.StartPosition = 0;
                DicomDecoder.Encode = FileEncode.ImplicitVR;
                DicomDictionary dict = new DicomDictionary();
                if (dict.DictLookup(GetString(buff, true)) != null)
                    return true;
                else
                    return false;
            }

        }
        /// <summary>
        /// Get the Length of the current element value field.
        /// </summary>
        /// <param name="count">Number of byte to be read</param>
        /// <returns>The number of bytes to read as long int</returns>
        public long GetLength(short count)
        {
            int length = 0;
            byte[] buffer = new byte[count];
            buffer = Reader(StartPosition, count);

            if (count == 2)
            {
                length = BitConverter.ToInt16(buffer, 0);
            }
            else
            {
                length = BitConverter.ToInt32(buffer, 0);
            }

            return length < 0 ? 0 : length;
        }
        /// <summary>
        /// Read the image pixels from the pixel data Item tag (7FE00010)
        /// </summary>
        public void ReadPixels(int toRead)
        {

            if (DicomObject.PixelSample == 1 && DicomObject.BitsAllocated == 8)
            {
                if (Pixels8 != null)
                    Pixels8.Clear();
                Pixels8 = new List<byte>();
                int numPixels = DicomObject.PixelColumns * DicomObject.PixelRows;
                byte[] buff = new byte[numPixels * 2];

                buff = Reader(StartPosition, toRead);
                PixelBuffer = buff;
                for (int i = 0; i < numPixels; ++i)
                {
                    int pixVal = (int)(buff[i] * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                    if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                        pixVal = max8 - pixVal;
                    Pixels8.Add((byte)(DicomObject.PixelRepresentation == 1 ? pixVal : (pixVal - min8)));
                }

            }
            if (DicomObject.PixelSample == 1 && DicomObject.BitsAllocated == 16)
            {
                if (Pixels16 != null)
                    Pixels16.Clear();
                if (Pixels16Int != null)
                    Pixels16Int.Clear();
                Pixels16 = new List<byte>();
                Pixels16Int = new List<int>();

                int numPixels = DicomObject.PixelColumns * DicomObject.PixelRows;

                byte[] bufByte = new byte[numPixels * 2];
                byte[] signedData = new byte[2];
                bufByte = Reader(StartPosition, numPixels * 2);
                ushort unsignedS;
                int i, i1, pixVal;
                byte b0, b1;
                PixelBuffer = bufByte;
                for (i = 0; i < numPixels; ++i)
                {
                    i1 = i * 2; b0 = bufByte[i1];
                    b1 = bufByte[i1 + 1];
                    unsignedS = Convert.ToUInt16((b1 << 8) + b0);
                    if (DicomObject.PixelRepresentation == 0)
                    {
                        pixVal = (int)(unsignedS * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                        if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    else
                    {
                        signedData[0] = b0;
                        signedData[1] = b1;
                        short sVal = BitConverter.ToInt16(signedData, 0);

                        pixVal = (int)(sVal * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                        if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    Pixels16Int.Add(pixVal);
                }
                int minPixVal = Pixels16Int.Min();
                DicomObject.SignedImage = false;
                if (minPixVal < 0)
                    DicomObject.SignedImage = true;

                foreach (int pixel in Pixels16Int)
                {
                    if (DicomObject.SignedImage)
                        Pixels16.Add((byte)(pixel - min16));
                    else
                        Pixels16.Add((byte)(pixel));
                }
                Pixels16Int.Clear();

            }

            if (DicomObject.PixelSample == 3 && DicomObject.BitsAllocated == 8)
            {
                DicomObject.SignedImage = false;
                if (Pixels24 != null)
                    Pixels24.Clear();
                Pixels24 = new List<byte>();
                int numPixels = DicomObject.PixelColumns * DicomObject.PixelRows;
                int numBytes = numPixels * DicomObject.PixelSample;
                byte[] myBuf = new byte[numBytes];
                myBuf = Reader(StartPosition, numBytes);

                PixelBuffer = myBuf;

                for (int i = 0; i < numBytes; ++i)
                {
                    Pixels24.Add(myBuf[i]);
                }
            }

        }
        public void ReadPixels(int toRead, int bAll, int columns, int rows)
        {
            if (DicomObject.PixelSample == 1 && bAll == 8)
            {
                if (Pixels8 != null)
                    Pixels8.Clear();
                Pixels8 = new List<byte>();
                int numPixels = columns * rows;
                byte[] buff = new byte[numPixels * 2];

                buff = Reader(StartPosition, toRead);
                PixelBuffer = buff;
                if (DicomImage.ImgPixel == null)
                    DicomImage.ImgPixel = new byte[buff.Length];

                for (int i = 0; i < numPixels; ++i)//numPixels
                {
                    int pixVal = (int)(buff[i] * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                    if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                        pixVal = max8 - pixVal;
                    Pixels8.Add((byte)(DicomObject.PixelRepresentation == 1 ? pixVal : (pixVal - min8)));
                    DicomImage.ImgPixel[i] = Pixels8[i];
                }

            }
            if (DicomObject.PixelSample == 1 && bAll == 16)
            {
                if (Pixels16 != null)
                    Pixels16.Clear();
                if (Pixels16Int != null)
                    Pixels16Int.Clear();
                Pixels16 = new List<byte>();
                Pixels16Int = new List<int>();

                int numPixels = columns * rows;

                byte[] bufByte = new byte[numPixels * 2];
                byte[] signedData = new byte[2];
                bufByte = Reader(StartPosition, numPixels * 2);
                PixelBuffer = bufByte;

                ushort unsignedS;
                int i, i1, pixVal;
                byte b0, b1;

                for (i = 0; i < numPixels; ++i)
                {
                    i1 = i * 2; b0 = bufByte[i1];
                    b1 = bufByte[i1 + 1];
                    unsignedS = Convert.ToUInt16((b1 << 8) + b0);
                    if (DicomObject.PixelRepresentation == 0)
                    {
                        pixVal = (int)(unsignedS * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                        if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    else
                    {
                        signedData[0] = b0;
                        signedData[1] = b1;
                        short sVal = BitConverter.ToInt16(signedData, 0);

                        pixVal = (int)(sVal * DicomObject.RescaleSlope + DicomObject.RescaleIntercept);
                        if (DicomObject.PhotoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    Pixels16Int.Add(pixVal);

                }
                int minPixVal = Pixels16Int.Min();
                DicomObject.SignedImage = false;
                if (minPixVal < 0)
                    DicomObject.SignedImage = true;
                int j = 0;
                DicomImage.ImgPixel = new byte[Pixels16Int.Count];
                foreach (int pixel in Pixels16Int)
                {
                    if (DicomObject.SignedImage)
                        Pixels16.Add((byte)(pixel - min16));
                    else
                        Pixels16.Add((byte)(pixel));
                    DicomImage.ImgPixel[j] = (byte)pixel;
                    j++;
                }
                Pixels16Int.Clear();

            }

            if (DicomObject.PixelSample == 3 && bAll == 8)
            {
                DicomObject.SignedImage = false;
                if (Pixels24 != null)
                    Pixels24.Clear();
                Pixels24 = new List<byte>();
                int numPixels = columns * rows;
                int numBytes = numPixels * DicomObject.PixelSample;
                byte[] myBuf = new byte[numBytes];
                myBuf = Reader(StartPosition, numBytes);
                PixelBuffer = myBuf;

                for (int i = 0; i < numBytes; ++i)
                {
                    Pixels24.Add(myBuf[i]);
                }
            }

        }


        public static byte[] GetLut(byte[] value)
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
        public void CloseReader()
        {
            this.myFileStreamer.Close();
            this.myFileStreamer.Dispose();
        }



        public static bool IsFloat { get; set; }

        public static bool IsBigEndian { get; set; }

        public static bool IsDate { get; set; }
    }
}
