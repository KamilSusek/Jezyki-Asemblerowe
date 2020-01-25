using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA
{
    /// <summary>
    ///  Controller class.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Input image copy.
        /// </summary>
        private Image img;

        /// <summary>
        /// Input file.
        /// </summary>
        private String file;

        /// <summary>
        /// SSE instructions availability flag.
        /// </summary>
        private bool isSSE;

        /// <summary>
        /// Init and SSE check.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            try
            {
                if (checkSSE() == 1)
                {
                    isSSE = true;
                }
                else
                {
                    isSSE = false;
                    MessageBox.Show("Your cpu does not support SSE instructions."
                                    + " You will not be able to perform any action");
                }
            }
            catch (DllNotFoundException dnfe)// dll does not exist
            {
                MessageBox.Show(dnfe.Message + " You will not be able to perform any action");
                isSSE = false;
            }

            label3.Text = "Threads count: " + Environment.ProcessorCount; //threads count
            trackBar1.Value = Environment.ProcessorCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport(@"dll\AsmDll.dll")]
        private static unsafe extern int checkSSE();

        /// <summary>
        /// Button performs load from file action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Images (*.BMP)|*.BMP|" + "All files (*.*)|*.*";
            DialogResult dr = dlg.ShowDialog();

            if (dr == DialogResult.OK)
            {
                file = dlg.FileName;
                Image image = new Bitmap(file);
                if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    Bitmap bmp = new Bitmap(file);
                    pictureBox1.Image = bmp;
                }
                else
                {
                    MessageBox.Show("Illegal format.");
                }
            }
            else
            {
                MessageBox.Show("Unable to load file");
            }
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (isSSE)
                    if (pictureBox1.Image != null)
                    {
                        img = new Bitmap(file);
                        
                        EdgeDetection algorithm = new EdgeDetection(img);
                        //cpp selected
                        if (checkBox1.CheckState == CheckState.Checked)
                        {
                            Image imageToSave;
                            bool libSelection = true;
                            ThreadManager threadsSet = new ThreadManager(trackBar1.Value, libSelection, ref algorithm);
                            threadsSet.CreateThreadsSet();
                            label4.Text = "Elapsed time: " + threadsSet.RunThreads() + " ms.";
                            pictureBox2.Image = algorithm.LoadToOutput();
                            imageToSave = pictureBox2.Image;
                            imageToSave.Save("output.bmp");
                            pictureBox2.Refresh();
                        }
                        else if (checkBox2.CheckState == CheckState.Checked)
                        {
                            Image imageToSave;
                            bool libSelection = false;
                            ThreadManager threadsSet = new ThreadManager(trackBar1.Value, libSelection, ref algorithm);
                            threadsSet.CreateThreadsSet();
                            label4.Text = "Elapsed time: " + threadsSet.RunThreads() + " ms.";
                            pictureBox2.Image = algorithm.LoadToOutput();
                            imageToSave = pictureBox2.Image;
                            imageToSave.Save("output.bmp");
                            //imageToSave.Dispose();
                            pictureBox2.Refresh();
                        }
                        else
                        {
                            MessageBox.Show("Select library!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No file is loaded!");
                    }
                else
                {
                    MessageBox.Show("Your cpu does not support SSE instructions or Dll files were not loaded properly."
                    + " You will not be able to perform any action.");
                }
            }
            catch (DllNotFoundException dnfe)
            {
                MessageBox.Show(dnfe.Message);
            }

        }

        /// <summary>
        /// Cpp checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
            {
                checkBox2.CheckState = CheckState.Unchecked;
            }
        }

        /// <summary>
        /// Asm checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.CheckState == CheckState.Checked)
            {
                checkBox1.CheckState = CheckState.Unchecked;
            }
        }

        /// <summary>
        /// Updating track bars value displayed in label3.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = "Threads count: " + trackBar1.Value;
        }

    }
}
