using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace PlateNumberExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }

        Bitmap img = null;
        

        private void button1_Click(object sender, EventArgs e)
        {
            


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Create filter sequence for segmentation process
            FiltersSequence fs1 = new FiltersSequence();
            fs1.Add(new Crop(new Rectangle(Convert.ToInt32(img.Width * 0.25), Convert.ToInt32(img.Height * 0.05), Convert.ToInt32(img.Width * 0.55), img.Height)));
            fs1.Add(new ResizeBilinear(700, 500));
            fs1.Add(new Grayscale(0.2125, 0.7154, 0.0721));
            fs1.Add(new CannyEdgeDetector(20, 40,0.5));
            Bitmap segmented = fs1.Apply(img);

            pictureBox2.Image = segmented;

            //Create outlined image
            FiltersSequence fs2 = new FiltersSequence();
            fs2.Add(new Crop(new Rectangle(Convert.ToInt32(img.Width * 0.25), Convert.ToInt32(img.Height * 0.05), Convert.ToInt32(img.Width * 0.55), img.Height)));
            fs2.Add(new ResizeBilinear(700, 500));
            Bitmap outlinedImg = fs2.Apply(img);
            Invert invFilter = new Invert();
            Bitmap inv = invFilter.Apply(segmented);
            for (int i = 0; i < inv.Width; i++)
            {
                for (int j = 0; j < inv.Height; j++)
                {
                    if (!(inv.GetPixel(i, j).R == 255) && !(inv.GetPixel(i, j).G == 255) && !(inv.GetPixel(i, j).B == 255))
                    {
                        outlinedImg.SetPixel(i, j, Color.White);
                    }
                }
            }

            pictureBox3.Image = outlinedImg;

            //Get the biggest blob with preset height & width
            BlobCounterBase bc = new BlobCounter();
            // set filtering options
            bc.FilterBlobs = true;
            bc.MinWidth = 4;
            bc.MinHeight = 3;
            bc.MaxHeight = 90;
            bc.MaxWidth = 500;
            // set ordering options
            bc.ObjectsOrder = ObjectsOrder.Size;
            // process binary image
            bc.ProcessImage(segmented);
            Blob[] blobs = bc.GetObjectsInformation();
            //MessageBox.Show(bc.ObjectsCount.ToString());
            // extract the biggest blob
            Bitmap blbImg = null;
            if (blobs.Length > 0)
            {
                bc.ExtractBlobsImage(outlinedImg, blobs[0], true);
                blbImg = blobs[0].Image.ToManagedImage();
            }

            pictureBox4.Image = blbImg;

            //Fill the biggest blob
            FiltersSequence fs3 = new FiltersSequence();
            fs3.Add(new Grayscale(0.2125, 0.7154, 0.0721));
            fs3.Add(new Dilatation());
            fs3.Add(new Threshold());
            fs3.Add(new FillHoles());
            Bitmap filledImg = fs3.Apply(blbImg);

            pictureBox5.Image = filledImg;

            //Remove plate number frame
            FiltersSequence fs4 = new FiltersSequence();
            for (int i = 0; i < 10; i++)
            {
                fs4.Add(new Erosion());
            }
            Bitmap removedFrameImg = fs4.Apply(filledImg);
            //Convert image to RGB
            GrayscaleToRGB img2RGB = new GrayscaleToRGB();
            Bitmap rgbImg = img2RGB.Apply(removedFrameImg);

            pictureBox6.Image = removedFrameImg;

            //Get plate number image
            Bitmap plateImg = fs2.Apply(img);
            for (int i = 0; i < rgbImg.Width; i++)
            {
                for (int j = 0; j < rgbImg.Height; j++)
                {
                    if (!(rgbImg.GetPixel(i, j).R == 255) && !(rgbImg.GetPixel(i, j).G == 255) && !(rgbImg.GetPixel(i, j).B == 255))
                    {
                        plateImg.SetPixel(i, j, Color.Black);
                    }
                }
            }

            pictureBox7.Image = plateImg;

            //Refine image
            FiltersSequence fs5 = new FiltersSequence();
            fs5.Add(new Grayscale(0.2125, 0.7154, 0.0721));
            fs5.Add(new Erosion());
            fs5.Add(new Threshold());
            fs5.Add(new Invert());
            fs5.Add(new Opening());
            fs5.Add(new Opening());
            fs5.Add(new Erosion());
            Bitmap finalPlateImg = fs5.Apply(plateImg);

            pictureBox8.Image = finalPlateImg;

            button1.Text = "Next image";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                img = new Bitmap(of.FileName);
                pictureBox1.Image = img;
                pictureBox2.Image = null;
                pictureBox3.Image = null;
                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                button2.Enabled = true;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
