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

//using System.Linq;
namespace DicomXml
{
    partial class DicomObject
    {
        public static int BitsAllocated;
        public static int PixelColumns;
        public static int PixelRows;
        //public static int offset;
        public static int PixelSample;
        public static int BitStored;
        public static short HighBit;
        public static short PixelRepresentation;
        public static double RescaleIntercept = 0;
        public static double RescaleSlope = 1;
        public static int ImgNumber;
       //Palette Color Lookup Table Descriptor
        public static int[] RedPaletteLookupTable { get; set; }
        public static int[] GreenPaletteLookupTable { get; set; }
        public static int[] BluePaletteLookupTable { get; set; }

        private static int numOfFrames = 0;

        public static int NumOfFrames { get { return numOfFrames; } set { numOfFrames = value; } }
       
        public static string TransferSyntax;

        public static double PixelDepth = 1.0;
        public static double PixelWidth = 1.0;
        public static double PixelHeight = 1.0;

        //public static string unit;
     
        public static string PhotoInterpretation;

        public static double WindowCentre, WindowWidth;
        public static bool SignedImage;
        //public static FileEncode Encode;
        //public List<string> dicomInfo;
        

        public List<DicomDataElement> MyDicomObject { get; set; }

        public DicomObject()
        {
            if (MyDicomObject != null)
                MyDicomObject.Clear();
            
            MyDicomObject = new List<DicomDataElement>();

            DefaultValue();
        }

        private void DefaultValue()
        {
            if (NumOfFrames != 0)
                NumOfFrames = 0;
            if (RedPaletteLookupTable != null)
            {
                RedPaletteLookupTable = null;
                GreenPaletteLookupTable = null;
                BluePaletteLookupTable = null;
            }
        }

        public void Dispose()
        {
            this.MyDicomObject.Clear();
        }





        public static int OverlayBitsAllocated { get; set; }

        public static int OverlayColumns { get; set; }

        public static int OverlayRows { get; set; }
    }
}
