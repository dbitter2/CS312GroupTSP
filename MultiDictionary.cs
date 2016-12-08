using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPTSP
{
    public struct SubProblem
    {
        public int subset;
        public int end;

        public SubProblem(int s, int e)
        {
            subset = s;
            end = e;
        }
    }

    public class MultiDictionary
    {
        private Dictionary<SubProblem, double> lookup;

        public MultiDictionary()
        {
            lookup = new Dictionary<SubProblem, double>();
        }

        public double get(int subset, int end)
        {
            SubProblem sub = new SubProblem(subset, end);
            if (lookup.ContainsKey(sub))
            {
                return lookup[sub];
            }
            else
            {
                return double.MaxValue;
            }
        }

        public void add(int subset, int end, double cost)
        {
            SubProblem sub = new SubProblem(subset, end);
            /*Boolean improvement = true;         
            foreach (SubProblem key in lookup.Keys)
            {
                if (key.subset == sub.subset && lookup[key] < cost)
                {
                    improvement = false;
                }
            }
            if(improvement)
            {
                lookup.Add(sub, cost);
            }*/
            lookup.Add(sub, cost);
        }

        public List<SubProblem> get(int subset)
        {
            List<SubProblem> subs = new List<SubProblem>();
            foreach(SubProblem sub in lookup.Keys)
            {
                if(sub.subset == subset)
                {
                    subs.Add(sub);
                }
            }
            return subs;
        }
    }
}
