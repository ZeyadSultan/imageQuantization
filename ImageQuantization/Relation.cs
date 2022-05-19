using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    class Relation : IComparable
    {
        public int src;
        public int dest;
        public double weight;



        public Relation(int src, int dest, double weight)
        {
            this.src = src;
            this.dest = dest;
            this.weight = weight;
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            return this.weight.CompareTo(((Relation)obj).weight);
        }
    }
}
