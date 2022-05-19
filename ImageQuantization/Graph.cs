using ImageQuantization;
using System;
using System.Collections.Generic;

class Graph
{
    static Dictionary<int, List<int>> graph =
            new Dictionary<int, List<int>>();


    public static void AddRel(List<Relation> l)
    {
        foreach (var rel in l)
        {
            AddEdge(rel.src, rel.dest);
            AddEdge(rel.dest, rel.src);
        }
    }

    public static void AddEdge(int a, int b)
    {
        if (graph.ContainsKey(a))
        {
            List<int> l = graph[a];
            l.Add(b);
            if (graph.ContainsKey(a))
                graph[a] = l;
            else
                graph.Add(a, l);
        }
        else
        {
            List<int> l = new List<int>();
            l.Add(b);
            graph.Add(a, l);
        }
    }

    public static List<RGBPixel> bfs(int s, List<bool> visited)
    {
        List<RGBPixel> cl = new List<RGBPixel>();

        List<int> q = new List<int>();

        q.Add(s);
        visited.RemoveAt(s);
        visited.Insert(s, true);

        while (q.Count != 0)
        {
            int x = q[0];
            q.RemoveAt(0);

            cl.Add(MainForm.distinctColors[x]);

            if (graph.ContainsKey(x))
            {

                for (int i = 0; i < graph[x].Count; i++)
                {
                    int n = graph[x][i];
                    if (!visited[n])
                    {
                        visited.RemoveAt(n);
                        visited.Insert(n, true);
                        q.Add(n);
                    }
                }
            }
        }

        return cl;
    }

    public static List<List<RGBPixel>> getClusters(int distinctColorsCount)
    {
        List<bool> vis = new List<bool>();
        List<List<RGBPixel>> clus = new List<List<RGBPixel>>();

        for (int i = 0; i < distinctColorsCount; i++)
        {
            vis.Insert(i, false);
        }
        for (int i = 0; i < distinctColorsCount; i++)
        {
            if (!vis[i])
            {

                List<RGBPixel> cl = bfs(i, vis);
                clus.Add(cl);
            }
        }
        return clus;
    }
}