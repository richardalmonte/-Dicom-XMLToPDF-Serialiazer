/*
 * This Software  and its supporting documantation were developed by 
 *      
 *      Richard Almonte
 *      
 *  As part of the project for internship at Sygest S.R.L, Parma Italy.
 *  Company's  tutor:  Stefano Maestri.  
 *  Accademic Tutor: Federico Bergenti
 *  
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;


namespace DicomXml
{
    /// <summary>
    /// The Main form class.
    /// </summary>
    public partial class mainFrm : Form
    {
        List<Bitmap> images;
        int i;
        double interval;
        /// <summary>
        /// Form Constructor.
        /// </summary>
        public mainFrm()
        {
            InitializeComponent();
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            string filename = openFile();
            if (filename == null)
                MessageBox.Show("No file have been selected. Please select one", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                txtPath.Text = filename;
                listView1.Items.Clear();
                GC.Collect();

                DicomDecoder decode = new DicomDecoder(filename);
                decode.processFile();

                foreach (var item in decode.dicomObject.myDicomObject)
                {
                    ListViewItem lvi = new ListViewItem(item.elementTag);
                    lvi.SubItems.Add(item.elementVR);
                    lvi.SubItems.Add(item.length.ToString());
                    lvi.SubItems.Add(item.tagDescription);
                    lvi.SubItems.Add(item.elementValue);
                    listView1.Items.Add(lvi);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                writeToXml xml = new writeToXml();

                xml.Write(decode.dicomObject.myDicomObject);
                images = new List<Bitmap>();
                images = decode.bmp;
                interval = decode.frameTime;
                decode.dispose();
            }
        }

        private string openFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            //fd.InitialDirectory = initialDirectoryPath;
            fd.Filter = "Dicom files (*.dcm) | *.dcm| All files (*.*)|*.*"; //file type options
            fd.FilterIndex = 2;
            fd.RestoreDirectory = true;
            fd.Title = "Select a DICOM file";
            //DialogResult dlgResult = fd.ShowDialog();

            if (fd.ShowDialog() == DialogResult.OK)
                return fd.FileName;
            else
                return null;
        }

        private void mainFrm_Load(object sender, EventArgs e)
        {
            DicomDictionary dict = new DicomDictionary();
            dict.createDicomDictionary();
        }

        private void cmdOpen_MouseClick(object sender, MouseEventArgs e)
        {
            this.timer1.Interval = interval <= 0.0 ? 2000 : Convert.ToInt32(interval);
            this.timer1.Tick += new EventHandler(this.timer1_Tick);
            this.timer1.Enabled = true;
            i = 0;
        }
       
       private void showImage(Bitmap image)
        {
            pictureBox1.Image = image;
        }

       private void timer1_Tick(object sender, EventArgs e)
       {
           showImage(images[i]);
           ++i;
           if (i >= images.Count)
               i = 0;
       }
    }
}
