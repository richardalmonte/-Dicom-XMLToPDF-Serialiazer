using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DicomXml
{
    class DicomImage
    {
        Bitmap bmp;
        byte[] palette8;
        byte[] palette16;
        
        int imgWidth = DicomObject.PixelColumns;
        int imgHeight = DicomObject.PixelRows;
        readonly int winWidth = Convert.ToInt32(DicomObject.WindowWidth);
        readonly int winCentre = Convert.ToInt32(DicomObject.WindowCentre);
        int winMax;
        int winMin;
        List<byte> pix8;
        List<byte> pix16;
        List<int> pix16Int;
        List<byte> pix24;

        public static byte[] ImgPixel { get; set; }

        public DicomImage()
        {
            int sizeImg = imgWidth * imgHeight;
            int sizeImg3 = sizeImg * 3;
            //GlobalAccess.Pixels8

            byte[] imgPixels8 = new byte[sizeImg3];
            //palette = ColorTable.Monochrome2;
            palette8 = null;
            palette16 = null;

            winMin = 0;
            winMax = 65535;

            if (bmp != null)
                bmp.Dispose();
            bmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //CreateImage();
        }

        public Bitmap CreateImage8()
        {
            bmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                int pixelSize = 3;
                int i, j, j1, i1;
                byte b;
                for (i = 0; i < bmd.Height; ++i)
                {
                    byte* row = (byte*)bmd.Scan0 + (i * bmd.Stride);
                    i1 = i * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = palette8[pix8[i * bmd.Width + j]];
                        j1 = j * pixelSize;
                        row[j1] = b;        // Red
                        row[j1 + 1] = b;    // Green
                        row[j1 + 2] = b;    // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);

            return bmp;
        }

        public Bitmap CreateImage16()
        {
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                int pixelSize = 3;
                int i, j, j1, i1;
                byte b;

                for (i = 0; i < bmd.Height; ++i)
                {
                    byte* row = (byte*)bmd.Scan0 + (i * bmd.Stride);
                    i1 = i * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = palette16[pix16[i * bmd.Width + j]];
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;
        }

        public Bitmap CreateImage24()
        {
            {
                int numBytes = imgWidth * imgHeight * 3;
                int j;
                int i, i1;

                BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                int width3 = bmd.Width * 3;

                unsafe
                {
                    for (i = 0; i < bmd.Height; ++i)
                    {
                        byte* row = (byte*)bmd.Scan0 + (i * bmd.Stride);
                        i1 = i * bmd.Width * 3;
                        for (j = 0; j < width3; j += 3)
                        {
                            row[j + 2] = palette8[pix24[i1 + j]];    //Blue
                            row[j + 1] = palette8[pix24[i1 + j + 1]];   //Green
                            row[j] = palette8[pix24[i1 + j + 2]];    //Red
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
        }

        public void ComputePalette8()
        {
            winMax = Convert.ToInt32(winCentre + 0.5 * winWidth);
            winMin = winMax - winWidth;
            if (winMax == 0)
                winMax = 255;

            int range = winMax - winMin;

            if (range < 1)
                range = 1;
            palette8 = new byte[65536];
            double factor = 255.0 / range;

            for (int i = 0; i < 256; ++i)
            {
                if (i <= winMin)
                    palette8[i] = 0;
                else if (i >= winMax)
                    palette8[i] = 255;
                else
                {
                    palette8[i] = (byte)((i - winMin) * factor);
                }
            }
        }

        public void ComputePalette16()
        {
            winMax = Convert.ToInt32(winCentre + 0.5 * winWidth);
            winMin = winMax - winWidth;
            if (winMax == 0)
                winMax = 255;

            int range = winMax - winMin;
            if (range < 1)
                range = 1;
            palette16 = new byte[65536];
            double factor = 255.0 / range;
            int i;

            for (i = 0; i < 65536; ++i)
            {
                if (i <= winMin)
                    palette16[i] = 0;
                else if (i >= winMax)
                    palette16[i] = 255;
                else
                    palette16[i] = (byte)((i - winMin) * factor);
            }
        }

        public void SaveImage(String fileName)
        {
            try
            {
                if (bmp != null)
                {
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    bmp.Save(fileName, jpgEncoder, myEncoderParameters /*ImageFormat.Png*/);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error = " + ex.Message);
            }
        }

        public string ToBase64String(Bitmap image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            byte[] imageArray = stream.ToArray();
            return Convert.ToBase64String(imageArray);
        }

        public Bitmap CreateImage()
        {
            if (DicomObject.PixelSample == 1 && DicomObject.BitsAllocated == 8)
            {
                pix8 = DicomReader.Pixels8;
                if (palette8 == null)
                    ComputePalette8();
                return CreateImage8();
            }
            else if (DicomObject.PixelSample == 1 && DicomObject.BitsAllocated == 16)
            {
                pix16 = DicomReader.Pixels16;
                pix16Int = DicomReader.Pixels16Int;
                ComputePalette16();
                return CreateImage16();
            }
            else if (DicomObject.PixelSample == 3 && DicomObject.BitsAllocated == 8)
            {
                pix24 = DicomReader.Pixels24;
                pix8 = DicomReader.Pixels8;
                return CreateImage24();
            }
            else
                return null;
        }

        public int SetValues(ImageType type, out int bitsAll, out int columns, out int rows)
        {
            int size = 0;

            switch (type)
            {
                case ImageType.OriginalImage:
                    bitsAll = DicomObject.BitsAllocated;
                    columns = DicomObject.PixelColumns;
                    rows = DicomObject.PixelRows;

                    size = GetFrameSize(DicomObject.BitsAllocated, DicomObject.PixelColumns, DicomObject.PixelRows);
                    break;
                case ImageType.Overlay:
                    bitsAll = DicomObject.BitsAllocated;
                    columns = DicomObject.OverlayColumns;
                    rows = DicomObject.OverlayRows;
                    size = GetFrameSize(bitsAll, columns, rows);
                    break;
                default:
                    bitsAll = DicomObject.BitsAllocated;
                    columns = DicomObject.PixelColumns;
                    rows = DicomObject.PixelRows;

                    break;
            }
            imgWidth = columns;
            imgHeight = rows;
            return size;
        }

        //prova
        //public BitmapSource decodeImage()
        //{
            
        //    MemoryStream imgBuff = new MemoryStream(ImgPixel);

        //    JpegBitmapDecoder decoder = new JpegBitmapDecoder(imgBuff, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

        //    //BitmapSource bitmapSource = decoder.Frames[0];

        //    return bitmapSource;
        //}
        private static byte[] TripletToPlanar(IList<byte> buffer, int width, int height)
        {
            var result = new byte[buffer.Count];

            int bytePlaneCount = width * height;
            for (int i = 0; i < bytePlaneCount; i++)
            {
                result[i] = buffer[i * 3];
                result[i + bytePlaneCount] = buffer[(i * 3) + 1];
                result[i + (2 * bytePlaneCount)] = buffer[(i * 3) + 2];
            }

            return result;
        }

        private int GetFrameSize(int bAll, int columns, int rows)
        {
            int frameSize = 0;
            if (bAll == 1)
            {
                var bytes = (columns * rows) / 8;
                if (((columns * rows) % 8) > 0)
                    bytes++;
                frameSize = bytes;
            }
            else if (DicomObject.NumOfFrames != 0)
                frameSize = DicomObject.PixelSample * columns * rows;
            else
            {
                frameSize = Convert.ToInt32(DicomReader.BytesToRead);
                DicomObject.NumOfFrames = 1;
            }
            return frameSize;
        }
        
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }
    }
}