using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    public class PQueue
    {
        private List<Solution> queue;
        private Dictionary<Solution, int> find; // Keeps track of which index each Solution is at
        private int maxStored;
        private int pruned;

        public PQueue()
        {
            queue = new List<Solution>();
            queue.Add(new Solution(false, null, null, double.MaxValue, null)); // Place holder to make the math for parent/child indices work
            find = new Dictionary<Solution, int>();
            maxStored = 0;
            pruned = 0;
        }

        public void Insert(Solution solution) // O(log(V))
        {
            queue.Add(solution);
            find[solution] = queue.Count - 1;
            BubbleUp(solution); // O(log(V))
            if(Count() > maxStored)
            {
                maxStored = Count();
            }
        }

        public void DecreaseKey(Solution solution) // O(log(V))
        {
            if (find.ContainsKey(solution))
            {
                BubbleUp(solution); // O(log(V))
            }
        }

        public Solution DeleteMin() // O(log(V))
        {
            Solution min = queue[1]; // Get minimum
            Solution last = queue[queue.Count - 1];
            Swap(min, last); // Swap the minimum and last vertices
            queue.RemoveAt(queue.Count - 1); // Remove minimum
            find.Remove(min);
            if (Count() > 1)
            {
                BubbleDown(last); // O(log(V))
            }
            return min;
        }

        public void BubbleUp(Solution solution) // O(log(V))
        {
            while (!Root(solution) && solution.priority() < queue[Parent(solution)].priority()) // Number of levels, log(V) because it's a heap
            {
                Swap(solution, queue[Parent(solution)]); // O(1)
            }
        }

        public void BubbleDown(Solution solution) // O(log(V))
        {
            while (!Leaf(solution) && solution.priority() > queue[MinChild(solution)].priority()) // Number of levels, log(V) because it's a heap
            {
                Swap(solution, queue[MinChild(solution)]); // O(1)
            }
        }

        public void Swap(Solution a, Solution b) // O(1)
        {
            int temp = find[a]; // Index of a
            find[a] = find[b]; // Change index of a to index of b
            find[b] = temp; // Change index of b to index of a
            queue[find[a]] = a; // Update a
            queue[find[b]] = b; // Update b
        }

        public int Parent(Solution solution) // O(1)
        {
            return find[solution] / 2;
        }

        public int MinChild(Solution solution) // O(1)
        {
            int left = LeftChild(solution);
            int right = RightChild(solution);
            double leftBound = (left < queue.Count) ? queue[left].priority() : double.MaxValue;
            double rightBound = (right < queue.Count) ? queue[right].priority() : double.MaxValue;
            return (leftBound <= rightBound) ? left : right;
        }

        public void Prune(double bound)
        {
            for(int i = 1; i < queue.Count; i++)
            {
                if(queue[i].priority() >= bound)
                {
                    Remove(queue[i]);
                    pruned++;
                }
            }
        }

        public void Remove(Solution solution)
        {
            Solution last = queue[queue.Count - 1];
            Swap(solution, last); // Swap the minimum and last vertices
            queue.RemoveAt(queue.Count - 1); // Remove minimum
            find.Remove(solution);
            if (Count() > 1)
            {
                BubbleDown(last); // O(log(V))
            }
        }

        public int LeftChild(Solution solution) // O(1)
        {
            return find[solution] * 2;
        }

        public int RightChild(Solution solution) // O(1)
        {
            return find[solution] * 2 + 1;
        }

        public bool Leaf(Solution solution)
        {
            return find[solution] * 2 > queue.Count - 1;
        }

        public bool Root(Solution solution)
        {
            return find[solution] == 1;
        }

        public int Count()
        {
            // Account for the placeholder
            return queue.Count - 1;
        }

        public int getMaxStored()
        {
            return maxStored;
        }

        public int getPruned()
        {
            return pruned;
        }

        public void addPruned(int add)
        {
            pruned += add;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < queue.Count; i++)
            {
                builder.Append((i < queue.Count - 1) ? "" + queue[i] + ", " : "" + queue[i]);
            }
            foreach (Solution solution in find.Keys)
            {
                builder.Append("\n" + solution + " : " + find[solution]);
            }
            builder.Append("\n");
            return builder.ToString();
        }
    }
}
