using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        public static int j = -1;
        public static int colorRed = 0;
        public static int colorBlue = 0;
        public static int colorGreen = 0;
        public static List<int>[] children;
        public static int[] rootNode;
        public static double[] colorWeight;
        public static double mst_total_cost = 0;
        public static List<KeyValuePair<RGBPixel, RGBPixel>> mst_tree = new List<KeyValuePair<RGBPixel, RGBPixel>>();
        public static RGBPixel[] clusterFinalColor;
        public static string[] dcolor;


        public static Stopwatch stopWatch;


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
            int sigma = int.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;

            //  ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

            RGBPixel[,] res_ImageMatrix = res(ImageMatrix, sigma);
            //NewColors(res_ImageMatrix);

            ImageOperations.DisplayImage(res_ImageMatrix, pictureBox2);
        }

        public static RGBPixel[,] res(RGBPixel[,] ImageMatrix, int numOfClusters)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
            get_distinct_colors(ImageMatrix);
            Console.WriteLine("get_distinct_colors: " + stopWatch.Elapsed);
            int[] rootNode = mst();
            Console.WriteLine("mst: " + stopWatch.Elapsed);

            quantizeimg(rootNode, numOfClusters, ImageMatrix);
            Console.WriteLine("quantizeimg: " + stopWatch.Elapsed);
            stopWatch.Stop();
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

        }



        //calculate distance function 
        public static double calc_dist(RGBPixel d1, RGBPixel d2)
        {
            int dist;
            dist = ((d1.red - d2.red) * (d1.red - d2.red)) + ((d1.green - d2.green) * (d1.green - d2.green)) + ((d1.blue - d2.blue) * (d1.blue - d2.blue));
            return dist;
        }


        // function to build the minimum spanning tree 
        public static int[] mst()
        {
            rootNode = new int[distinctColors.Count];
            colorWeight = new double[distinctColors.Count];

            bool[] visited = new bool[distinctColors.Count];

            for (int i = 0; i < distinctColors.Count; i++)
            {
                colorWeight[i] = double.MaxValue;
            }
            rootNode[0] = -1;
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
            children = new List<int>[distinctColors.Count];
            return rootNode;

        }

        public static void quantizeimg(int[] rootNode, int numOfClusters, RGBPixel[,] ImageMatrix)
        {
            List<Relation> l = new List<Relation>();
            for (int i = 1; i < rootNode.Length; i++)
            {
                Relation r = new Relation(rootNode[i], i, colorWeight[i]);
                l.Add(r);
            }
            l.Sort();
            for (int i = 0; i < numOfClusters - 1; i++)
            {
                l.RemoveAt(l.Count - 1);
            }

            Graph.AddRel(l);
            List<List<RGBPixel>> clus = Graph.getClusters(distinctColors.Count);
            List<RGBPixel> newColorFromIndex = new List<RGBPixel>();
            for (int i = 0; i < clus.Count; i++)
            {
                int red = 0, blue = 0, green = 0;
                for (int j = 0; j < clus[i].Count; j++)
                {
                    RGBPixel currPixel = clus[i][j];
                    red += currPixel.red;
                    blue += currPixel.blue;
                    green += currPixel.green;
                }
                RGBPixel newp;
                //newp.red = (byte)Math.Ceiling((int)red / clus[i].Count);
                newp.red = (byte)(red / clus[i].Count);
                newp.blue = (byte)(blue / clus[i].Count);
                newp.green = (byte)(green / clus[i].Count);
                newColorFromIndex.Add(newp);
            }
            Dictionary<int, int> pixelClusterIndex = new Dictionary<int, int>();
            for (int i = 0; i < clus.Count; i++)
            {
                var list = clus[i];
                for (int j = 0; j < list.Count; j++)
                {
                    RGBPixel currPixel = list[j];
                    pixelClusterIndex.Add(pixelToInt(currPixel), i);
                }

            }

            int length = ImageMatrix.GetLength(0);
            int width = ImageMatrix.GetLength(1);
            for (long i = 0; i < length; i++)
            {
                for (long j = 0; j < width; j++)
                {
                    RGBPixel oldP = ImageMatrix[i, j];
                    int clusIndexOld = pixelClusterIndex[pixelToInt(oldP)];
                    RGBPixel newP = newColorFromIndex[clusIndexOld];
                    ImageMatrix[i, j] = newP;
                }
            }

        }


        static void printblez(List<List<RGBPixel>> clus)
        {
            foreach (var l in clus)
            {
                Console.WriteLine("clsuter number ");
                foreach (var n in l)
                {
                    Console.WriteLine("rgb(" + n.red.ToString() + "," + n.green.ToString() + "," + n.blue.ToString() + ")");
                }
                Console.WriteLine();
                Console.WriteLine("--------------------------------------");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }


        public static int pixelToInt(RGBPixel color)
        {
            int a = 0;
            int r = color.red;
            int g = color.green;
            int b = color.blue;

            return a << 24 | r << 16 | g << 8 | b;
        }

        public static RGBPixel intToPixel(int argb)
        {
            int a = argb >> 24;
            int r = argb << 8 >> 24;
            int g = argb << 16 >> 24;
            int b = argb << 24 >> 24;
            RGBPixel p;
            p.red = (byte)r;
            p.blue = (byte)b;
            p.green = (byte)g;
            return p;
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}