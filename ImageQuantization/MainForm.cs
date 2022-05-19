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
        public static List<RGBPixel> distinctColors = new List<RGBPixel>();
        public static Dictionary<RGBPixel, int> colorCluster = new Dictionary<RGBPixel, int>();
        public static byte clusterSize;
        public static int j = 0;
        public static int colorRed = 0;
        public static int colorBlue = 0;
        public static int colorGreen = 0;
        public static List<int>[] children;
        public static int[] rootNode;
        public static double[] colorWeight;
        public static double mst_total_cost = 0;
        public static List<KeyValuePair<RGBPixel, RGBPixel>> mst_tree = new List<KeyValuePair<RGBPixel, RGBPixel>>();
        public static RGBPixel[] clusterFinalColor;



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
            int maskSize = (int)nudMaskSize.Value;

            //  ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            NewColors(ImageMatrix);

            RGBPixel[,] res_ImageMatrix = res(ImageMatrix, maskSize);

            ImageOperations.DisplayImage(res_ImageMatrix, pictureBox2);
        }

        public static RGBPixel[,] res(RGBPixel[,] ImageMatrix, int numOfClusters)
        {
            get_distinct_colors(ImageMatrix);
            mst();
            makeClusters(ImageMatrix, numOfClusters);
            return ImageMatrix;
        }

        public static void get_distinct_colors(RGBPixel[,] ImageMatrix)
        {
            bool[] checkedColor = new bool[100000000];

            long index;

            for (int h = 0; h < ImageMatrix.GetLength(0); h++)
            {
                for (int w = 0; w < ImageMatrix.GetLength(1); w++)
                {

                    index = ImageMatrix[h, w].red + ImageMatrix[h, w].green * 256 + ImageMatrix[h, w].blue * 256 * 256;

                    if (checkedColor[index] == false)
                    {
                        distinctColors.Add(ImageMatrix[h, w]);
                        checkedColor[index] = true;
                    }

                }
            }

            Console.WriteLine(distinctColors.Count);
            rootNode = new int[distinctColors.Count];

        }


        //calculate distance function 
        public static double calc_dist(RGBPixel d1, RGBPixel d2)
        {
            int dist;
            dist = ((d1.red - d2.red) * (d1.red - d2.red)) + ((d1.green - d2.green) * (d1.green - d2.green)) + ((d1.blue - d2.blue) * (d1.blue - d2.blue));
            return dist;
        }

        
        // function to build the minimum spanning tree 
        public static void mst()
        {
            colorWeight = new double[distinctColors.Count];

            bool[] visited = new bool[distinctColors.Count];

            for (int i = 0; i < distinctColors.Count; i++)
            {
                colorWeight[i] = double.MaxValue;
            }
            rootNode[0] = 0;
            colorWeight[0] = 0;
            for (int j = 0; j < distinctColors.Count; j++)
            {
                double leastColorWeight = double.MaxValue;
                int rootInd = 0;
                for (int k = 0; k < distinctColors.Count; k++)
                {
                    if (visited[k] == false && colorWeight[k] < leastColorWeight)
                    {
                        leastColorWeight = colorWeight[k];
                        rootInd = k;
                    }
                }

                visited[rootInd] = true;

                for (int z = 1; z < distinctColors.Count; z++)
                {
                    if (calc_dist(distinctColors[z], distinctColors[rootInd]) > 0 && visited[z] == false && calc_dist(distinctColors[z], distinctColors[rootInd]) < colorWeight[z])
                    {
                        colorWeight[z] = (calc_dist(distinctColors[z], distinctColors[rootInd]));
                        rootNode[z] = rootInd;
                    }
                }

            }

            for (int i = 0; i < colorWeight.Length; i++)
            {
                mst_total_cost += Math.Sqrt(colorWeight[i]);
            }
            Console.WriteLine("mst cost : " + mst_total_cost);

        }

        public static void makeClusters(RGBPixel[,] ImageMatrix, int numOfClusters)
        {
            clusterFinalColor = new RGBPixel[numOfClusters];
            for (int i = 0; i < numOfClusters - 1; i++)
            {
                double maxWeight = 0;
                int hekha = 0;
                for (int j = 0; j < distinctColors.Count; j++)
                {
                    if (colorWeight[j] > maxWeight)
                    {
                        maxWeight = colorWeight[j];
                        hekha = j;
                    }
                }
                rootNode[hekha] = hekha;
                colorWeight[hekha] = -1;
            }
        }
        public static string[] color = new string[distinctColors.Count];

        public static void makeChildren(RGBPixel[,] ImageMatrix, int numOfClusters)
        {
            //adding the children of each node in the clusters
            children = new List<int>[distinctColors.Count];
            for (int i = 0; i < distinctColors.Count; i++)
            {
                children[i] = new List<int>(distinctColors.Count);
            }
            for (int i = 0; i < distinctColors.Count; i++)
            {
                color[i] = "white";
                if (rootNode[i] != i)
                {
                    children[i].Add(rootNode[i]);
                    children[rootNode[i]].Add(i);
                }
            }



            for (int i = 0; i < distinctColors.Count; i++)
            {
                if (color[i] == "white")
                {
                    clusterSize = 1;
                    j++;
                    colorRed = 0;
                    colorBlue = 0;
                    colorGreen = 0;
                    dfs(i);
                    
                    colorRed = colorRed / clusterSize;

                    colorBlue = colorBlue / clusterSize;

                    colorGreen = colorGreen / clusterSize;

                    colorCluster[distinctColors[i]] = j;

                    clusterFinalColor[j].red = (byte)colorRed;
                    clusterFinalColor[j].blue = (byte)colorBlue;
                    clusterFinalColor[j].green = (byte)colorGreen;
                    /*colorCluster[i].blue = (byte)colorBlue;
                    colorCluster[i].red = (byte)colorRed;
                    colorCluster[i].green = (byte)colorGreen;*/
                }
            }

            

            for (int i = 0; i < colorCluster.Count; i++)
            {
                

                for (int j = 0; j < numOfClusters; j++)
                {


                    /*colorRed += ((int)colorCluster[i][j].red);

                    colorBlue += ((int)colorCluster[i][j].blue);

                    colorGreen += ((int)colorCluster[i][j].green);*/

                }
                /*clusterFinalColor[i].red = (byte)(colorRed / clusterSize);
                clusterFinalColor[i].blue = (byte)(colorBlue / clusterSize);
                clusterFinalColor[i].green = (byte)(colorGreen / clusterSize);*/
            }

            


            for (int i = 0; i < distinctColors.Count; i++)
            {
                for(int j = 0; j < numOfClusters; j++)
                {
                    if(colorCluster[distinctColors[i]] == j)
                    {
                        distinctColors[i] = clusterFinalColor[j];
                    }
                }
            }


            

            


        }




        /// <summary>
        /// ///////////////////////////////////////////////////////
        public static RGBPixel[,] NewColors(RGBPixel[,] ImageMatrix)
        {
            RGBPixel[,,] RGB = new RGBPixel[126, 126, 126];
            /*for (int x = 0; x < distinctColors.Count; x++)
            {
                RGBPixel Clr_map = distinctColors[x];
                RGB[distinctColors[x].red, distinctColors[x].green, distinctColors[x].blue] = Clr_map;
            }*/
            int length = ImageMatrix.GetLength(0);
            int width = ImageMatrix.GetLength(1);
            for (long i = 0; i < length; i++)
            {
                for (long j = 0; j < width; j++)
                {
                    RGBPixel new_Clr;
                    new_Clr = RGB[125, 0, 0];
                    ImageMatrix[i, j].red = new_Clr.red;
                    ImageMatrix[i, j].green = new_Clr.green;
                    ImageMatrix[i, j].blue = new_Clr.blue;
                }
            }
            return ImageMatrix;
        }
        /// </summary>
        /// <param name="s"></param>





        public static void dfs(int s)//s=0
        {
 
            clusterSize++;
            color[s] = "gray";
            colorCluster[distinctColors[s]] = j;

            colorRed += distinctColors[s].red;
            colorBlue += distinctColors[s].blue;
            colorGreen += distinctColors[s].green;

            foreach (int i in children[s])
            {
                if (color[i] == "white")
                {
                    dfs(i);
                }

                if (color[i] == "gray")
                {
                    break;
                }
            }


            

            color[s] = "black";

        }


        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}