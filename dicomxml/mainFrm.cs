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
using System.Drawing;
//using System.Linq;
using System.Windows.Forms;

namespace DicomXml
{
    /// <summary>
    /// The Main form class.
    /// </summary>
    public partial class MainFrm : Form
    {
        List<Bitmap> images;
        int i;
        double interval;
        /// <summary>
        /// Form Constructor.
        /// </summary>
        public MainFrm()
        {
            InitializeComponent();
        }

        private void CmdOpen_Click(object sender, EventArgs e)
        {
            string filename = OpenFile();
            if (filename == null)
                MessageBox.Show("No file have been selected. Please select one", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                txtPath.Text = filename;
                listView1.Items.Clear();
                GC.Collect();
                DicomImage.ImgPixel = DicomImage.ImgPixel != null ? null : DicomImage.ImgPixel;
                DicomDecoder decode = new DicomDecoder(filename);
                decode.ProcessFile();

                foreach (var item in decode.DicomObject.MyDicomObject)
                {
                    ListViewItem lvi = new ListViewItem(item.ElementTag);
                    lvi.SubItems.Add(item.ElementVR);
                    lvi.SubItems.Add(item.Length.ToString());
                    lvi.SubItems.Add(item.TagDescription);
                    lvi.SubItems.Add(item.ElementValue);
                    listView1.Items.Add(lvi);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                WriteToXml xml = new WriteToXml();

                xml.Write(decode.DicomObject.MyDicomObject);
                images = new List<Bitmap>();
                images = decode.Bmp;
                interval = decode.FrameTime;
                decode.Dispose();
            }
        }

        private string OpenFile()
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

        private void MainFrm_Load(object sender, EventArgs e)
        {
            DicomDictionary dict = new DicomDictionary();
            dict.CreateDicomDictionary();
        }

        private void CmdOpen_MouseClick(object sender, MouseEventArgs e)
        {
            this.timer1.Interval = interval <= 0.0 ? 2000 : Convert.ToInt32(interval);
            this.timer1.Tick += new EventHandler(this.Timer1_Tick);
            this.timer1.Enabled = true;
            i = 0;
        }
       
       private void ShowImage(Bitmap image)
        {
            pictureBox1.Image = image;
        }

       private void Timer1_Tick(object sender, EventArgs e)
       {
           ShowImage(images[i]);
           ++i;
           if (i >= images.Count)
               i = 0;
       }
    }
}
