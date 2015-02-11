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
using System.Text;

namespace DicomXml
{
    /// <summary>
    /// Using the TransferSyntaxUID tag (0002,0010) to identify the file encoding mode.
    /// </summary>
    public enum FileEncode
    {
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2
        /// </summary>
        implicitVR,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.1 
        /// </summary>
        explicitLittle,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.2
        /// </summary>
        explicitBig,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.4.91
        /// </summary>
        jpeg
    }

   partial class  DicomObject
    {
        public static int bitsAllocated;
        public static int pixelColumns;
        public static int pixelRows;
        //public static int offset;
        public static int pixelSample;
        public static int bitStored;
        public static short highBit;
        public static short pixelRepresentation;
        public static double rescaleIntercept = 0;
        public static double rescaleSlope = 1;
        public static int imgNumber;
       //Palette Color Lookup Table Descriptor
        public static int[] RedPaletteLookupTable { get; set; }
        public static int[] GreenPaletteLookupTable { get; set; }
        public static int[] BluePaletteLookupTable { get; set; }

        private static int _numOfFrames = 0;

        public static int numOfFrames { get { return _numOfFrames; } set { _numOfFrames = value; } }
       
        public static string transferSyntax;

        public static double pixelDepth = 1.0;
        public static double pixelWidth = 1.0;
        public static double pixelHeight = 1.0;

        //public static string unit;
     
        public static string photoInterpretation;

        public static double windowCentre, windowWidth;
        public static bool signedImage;
        //public static FileEncode encode;
        //public List<string> dicomInfo;
        

        public List<DicomDataElement> myDicomObject { get; set; }

        public DicomObject()
        {
            if (myDicomObject != null)
                myDicomObject.Clear();
            
            myDicomObject = new List<DicomDataElement>();

            defaultValue();
        }

        private void defaultValue()
        {
            if (numOfFrames != 0)
                numOfFrames = 0;
            if (RedPaletteLookupTable != null)
            {
                RedPaletteLookupTable = null;
                GreenPaletteLookupTable = null;
                BluePaletteLookupTable = null;
            }
        }

        public void dispose()
        {
            this.myDicomObject.Clear();
        }





        public static int OverlayBitsAllocated { get; set; }

        public static int OverlayColumns { get; set; }

        public static int OverlayRows { get; set; }
    }
}
