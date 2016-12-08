using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    public class Solution
    {
        private bool complete;
        private List<City> route;
        private double[][] costs; // n^2 space
        private double bound;
        private City[] cities;

        public Solution(bool complete, List<City> route, double[][] costs, double bound, City[] cities)
        {
            this.complete = complete;
            this.route = route;
            this.costs = costs;
            this.bound = bound;
            this.cities = cities;
        }

        public List<Solution> getNeighbors()
        {
            int from = Array.IndexOf(cities, route[route.Count - 1]);
            List<Solution> neighbors = new List<Solution>();
            for(int to = 0; to < costs.Length; to++) // b
            {
                if (costs[from][to] != double.MaxValue)
                {
                    List<City> newRoute = new List<City>(route);
                    newRoute.Add(cities[to]);
                    double[][] newCosts = copy(costs);
                    for (int row = 0; row < newCosts.Length; row++) // n
                    {
                        newCosts[row][to] = double.MaxValue;
                    }
                    for (int col = 0; col < newCosts[from].Length; col++) // n
                    {
                        newCosts[from][col] = double.MaxValue;
                    }
                    newCosts[to][from] = double.MaxValue;
                    Solution neighbor = new Solution(false, newRoute, newCosts, bound + costs[from][to], cities);
                    neighbor.reduce(); // O(n^2)
                    neighbors.Add(neighbor);
                }
            }
            if(neighbors.Count == 1)
            {
                neighbors[0].setComplete(true);
            }
            return neighbors;
        }

        public void reduce() // O(n^2)
        {
            // Eliminate row
            for(int row = 0; row < costs.Length; row++) // n
            {
                double min = rowMin(row); // n
                if(min != 0 && min != double.MaxValue)
                {
                    bound += min;
                    for (int col = 0; col < costs[row].Length; col++) // n
                    {
                        costs[row][col] = (costs[row][col] == double.MaxValue) ? double.MaxValue : costs[row][col] - min;
                    }
                }                       
            }
            // Eliminate column
            for(int col = 0; col < costs[0].Length; col++) // n
            {
                double min = colMin(col); // n
                if(min != 0 && min != double.MaxValue)
                {
                    bound += min;
                    for (int row = 0; row < costs.Length; row++) // n
                    {
                        costs[row][col] = (costs[row][col] == double.MaxValue) ? double.MaxValue : costs[row][col] - min;
                    }
                }
            }
        }

        public double rowMin(int row) // O(n)
        {
            double min = costs[row][0];
            for(int i = 1; i < costs.Length; i++) // n
            {
                if(costs[row][i] < min)
                {
                    min = costs[row][i];
                }
            }
            return min;
        }

        public double colMin(int col) // O(n)
        {
            double min = costs[0][col];
            for(int i = 1; i < costs[0].Length; i++) //n
            {
                if(costs[i][col] < min)
                {
                    min = costs[i][col];
                }
            }
            return min;
        }

        public void printCosts()
        {
            for (int i = 0; i < costs.Length; i++)
            {
                for (int j = 0; j < costs[i].Length; j++)
                {
                    Console.Write("" + costs[i][j] + " ");
                }
                Console.WriteLine();
            }
        }

        public double[][] copy(double[][] old) // O(n^2)
        {
            double[][] newArray = new double[old.Length][];
            for(int i = 0; i < old.Length; i++) // n
            {
                newArray[i] = new double[old[0].Length];
                for(int j = 0; j < old[i].Length; j++) // n
                {
                    newArray[i][j] = old[i][j];
                }
            }
            return newArray;
        }

        public double priority()
        {
            return bound / route.Count;
        }

        public void setComplete(bool complete)
        {
            this.complete = complete;
        }

        public bool isComplete()
        {
            return complete;
        }

        public List<City> getRoute()
        {
            return route;
        }

        public double[][] getCosts()
        {
            return costs;
        }

        public double getBound()
        {
            return bound;
        }
    }
}