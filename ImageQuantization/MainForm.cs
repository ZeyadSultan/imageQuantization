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
        // public static int distinct_colors = 0;
      
     /*   public static RGBPixel[] distColors = new RGBPixel[100000];
        public static double[,] edgeWeight = new double[distinct_colors.Count, distinct_colors.Count];
        public static List<KeyValuePair<int, double>>[] vertixEdges = new List<KeyValuePair<int, double>>[distinct_colors.Count]; */


        public static List<RGBPixel> distinct_colors = new List<RGBPixel>();
        public static Dictionary<RGBPixel,List< RGBPixel>> edges = new Dictionary<RGBPixel, List<RGBPixel>>();
        
        public static List<KeyValuePair<RGBPixel , RGBPixel>> mst_tree = new List<KeyValuePair<RGBPixel,RGBPixel> > ();
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

            //  ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
           RGBPixel[,] res_ImageMatrix =  res(ImageMatrix);
            
            ImageOperations.DisplayImage(res_ImageMatrix, pictureBox2);
        }

        public static RGBPixel[,] res (RGBPixel [,] ImageMatrix)
        {
            get_distinct_colors(ImageMatrix);
            mst();
            return ImageMatrix;
        }

    /*   public static void findDistinctColors(RGBPixel[,] imagePixels)
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
        }*/
      /*  public static void createGraph()
        {
            for(int i =0;i<distinct_colors.Count;i++)
            {
                for(int j=0;j<distinct_colors.Count;j++)
                {
                    if(i != j)
                    {
                        edgeWeight[i, j] = Math.Sqrt(Math.Pow((distColors[i].red) - (distColors[j].red), 2) +
                            Math.Pow((distColors[i].blue) - (distColors[j].blue), 2) +
                            Math.Pow((distColors[i].green) - (distColors[j].green), 2));
                        vertixEdges[i].Add(new KeyValuePair<int, double>(j, edgeWeight[i, j]));
                    }
                }
            }
        }*/
        

     
            public static void get_distinct_colors (RGBPixel[,] ImageMatrix)
        {
            bool[] marked_distinct_color = new bool[100000000];

            long  index;
            
            for (int h = 0; h < ImageMatrix.GetLength(0); h++)
            {
                for (int w = 0; w < ImageMatrix.GetLength(1); w++)
                {
                    
                    index = ImageMatrix[h, w].red + ImageMatrix[h, w].green * 256 + ImageMatrix[h, w].blue * 256 * 256;
                     
                    if (marked_distinct_color[index] == false) 
                    {
                        distinct_colors.Add(ImageMatrix[h, w]);
                        marked_distinct_color[index] = true; 
                    }
                  
                }
            }
            Console.WriteLine(distinct_colors.Count);
        }


      


        //calculate distance function 
        public static double calc_dist (RGBPixel d1 , RGBPixel d2)
        {
            int dist;
            dist = ((d1.red - d2.red) * (d1.red - d2.red)) + ((d1.green - d2.green) * (d1.green - d2.green)) + ((d1.blue - d2.blue) * (d1.blue - d2.blue));
            return dist;
        }
        // function to build the minimum spanning tree 

        public static void mst()
        {
            int distinct_num = distinct_colors.Count;


            bool[] visited = new bool[distinct_num];

            double[] color_weights = new double[distinct_num];
            double mst_total_cost = 0;
            for (int i = 0; i < distinct_num; i++)
            {
                color_weights[i] = double.MaxValue;

               

            }
            color_weights[0] = 0;
            for (int j = 0; j < distinct_num; j++)
                {
                    double Minimumvalue = double.MaxValue;
                    int root_ind = 0;
                    for (int k = 0; k < distinct_colors.Count; k++)
                    {
                        if (visited[k] == false && color_weights[k] < Minimumvalue)
                        {
                            Minimumvalue = color_weights[k];
                            root_ind = k;
                        }
                    }

                    visited[root_ind] = true;



                    for (int z = 0; z < distinct_num; z++)
                    {

                        if (calc_dist(distinct_colors[z], distinct_colors[root_ind]) > 0 && visited[z] == false && calc_dist(distinct_colors[z], distinct_colors[root_ind]) < color_weights[z])
                        {

                            color_weights[z] = ( calc_dist(distinct_colors[z], distinct_colors[root_ind]));
                        }
                    }

                }
               

            

            for (int i = 0; i < color_weights.Length; i++)
            {
                mst_total_cost += Math.Sqrt( color_weights[i]);
            }
            Console.WriteLine("mst cost : " + mst_total_cost);

        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
