using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    
    public partial class MainForm : Form
    {
        public static int distinct_colors = 0;
        public static RGBPixel[] distColors = new RGBPixel[100000];
        public static double[,] edgeWeight = new double[distinct_colors, distinct_colors];
        public static List<KeyValuePair<int, double>>[] vertix = new List<KeyValuePair<int, double>>[distinct_colors];
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        public static void findDistinctColors(RGBPixel[,] imagePixels)
        {
            bool[,,] visited = new bool[255,255,255];
            for(int i = 0;i<imagePixels.GetLength(0);i++)
            {
                for(int j=0;j<imagePixels.GetLength(1);j++)
                {
                    visited[imagePixels[i, j].red, imagePixels[i, j].green, imagePixels[i, j].blue] = true;
                }
            }

            for (int r=0;r<256; r++)
            {
                for(int g=0;g<256; g++)
                {
                    for(int  b=0;b<256;b++)
                    {
                        if(visited[r,g,b])
                        {
                            RGBPixel pixel;
                            pixel.red = (byte)r;
                            pixel.green = (byte)g;
                            pixel.blue = (byte)b;

                            distColors[distinct_colors] = pixel;
                            distinct_colors++;
                        }
                    }
                }
            }
        }
        public static void createGraph()
        {
            for(int i =0;i<distinct_colors;i++)
            {
                for(int j=0;j<distinct_colors;j++)
                {
                    if(i != j)
                    {
                        edgeWeight[i, j] = Math.Sqrt(Math.Pow((distColors[i].red) - (distColors[j].red), 2) +
                            Math.Pow((distColors[i].blue) - (distColors[j].blue), 2) +
                            Math.Pow((distColors[i].green) - (distColors[j].green), 2));
                        vertix[i].Add(new KeyValuePair<int, double>(j, edgeWeight[i, j]));
                    }
                }
            }
        }
    }
}
