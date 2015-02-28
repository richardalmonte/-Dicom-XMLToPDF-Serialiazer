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
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;

namespace DicomXml
{
   public class WriteToXml
    {
        private readonly string filename;

        public WriteToXml()
        {
            filename = ConfigurationManager.AppSettings["xmlPath"];
        }
        /// <summary>
        /// Write the XML file.
        /// </summary>
        /// <param name="dicom">List of DicomELements</param>
        public void Write(List<DicomDataElement> dicom)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.Encoding = Encoding.Unicode;
            

            XmlTextWriter xmlWriter = new XmlTextWriter(filename, Encoding.Unicode);

            xmlWriter.Formatting = Formatting.Indented;
            
            myDicomObject xml = new myDicomObject();
            DicomObjectElement de = new DicomObjectElement();
            DicomObjectImage img = new DicomObjectImage();

            xml.Element = new List<DicomObjectElement>();
            xml.Image = new List<DicomObjectImage>();
            short i;
            foreach (DicomDataElement element in dicom)
            {
                de = new DicomObjectElement();
                de.tag = element.ElementTag;
                de.vr = element.ElementVR;
                de.description = element.TagDescription;
                de.length = element.Length;
                de.value = element.ElementValue;
                i = 0;
                foreach (var image in element.FrameValue)
                {
                    img = new DicomObjectImage();
                    img.Frame = i;
                    img.Value = image;
                    xml.Image.Add(img);
                    ++i;
                }
                xml.Element.Add(de);

            }
            XmlSerializer serializer = new XmlSerializer(typeof(myDicomObject));//, "DicomObject");
            try
            {
                
                serializer.Serialize(xmlWriter, xml);
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }

            xmlWriter.Close();
        }
    }
}
