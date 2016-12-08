using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using DPTSP;
using System.Linq;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        //**********************************************BBAlgorithm*********************************************************************************
        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem() // O(n^2b^n)
        {
            string[] results = new string[3];
            Stopwatch timer = new Stopwatch();
            timer.Start();

            int solutions = 0;
            int updates = 0;
            int states = 1;
            greedySolveProblem(); // O(n^3)
            Solution best = new Solution(true, new List<City>(bssf.Route.ToArray(typeof(City)) as City[]), null, costOfBssf(), null);
            double[][] costs = new double[Cities.Length][];
            for (int from = 0; from < costs.Length; from++) // n
            {
                costs[from] = new double[Cities.Length];
                for (int to = 0; to < costs[from].Length; to++) // n
                {
                    costs[from][to] = (from == to) ? double.MaxValue : Cities[from].costToGetTo(Cities[to]);
                }
            }
            List<City> route = new List<City>();
            route.Add(Cities[0]);
            Solution partial = new Solution(false, route, costs, 0, Cities);
            partial.reduce();
            PQueue pq = new PQueue();
            pq.Insert(partial);
            while (timer.ElapsedMilliseconds < time_limit && pq.Count() > 0) // b^n (b = branching factor) (number of states in the PQ)
            {
                partial = pq.DeleteMin(); // Highest Priority partial solution
                if (partial.getBound() < best.getBound()) // Make sure the bssf wasn't updated to make this one invalid
                {
                    List<Solution> neighbors = partial.getNeighbors(); // O(bn^2)
                    foreach (Solution solution in neighbors) //b
                    {
                        states++;
                        if (solution.isComplete())
                        {
                            solutions++;
                            if (solution.getBound() < best.getBound())
                            {
                                updates++;
                                best = solution;
                            }
                        }
                        else
                        {
                            if (solution.getBound() < best.getBound())
                            {
                                pq.Insert(solution);
                            }
                            else
                            {
                                pq.addPruned(1);
                            }
                        }
                    }
                }
                else
                {
                    pq.addPruned(1);
                }
            }
            pq.addPruned(pq.Count());

            timer.Stop();


            bssf.Route = new ArrayList(best.getRoute());
            results[COST] = costOfBssf().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = solutions.ToString();

            Console.WriteLine("stored: " + pq.getMaxStored());
            Console.WriteLine("updates: " + updates);
            Console.WriteLine("states: " + states);
            Console.WriteLine("pruned: " + pq.getPruned());
            Console.WriteLine();

            return results;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////
        //************************************************************************Greedy Algorithm*****************************************************************************************************
        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        /// Dallas'
        public string[] greedySolveProblem() // O(n^2)
        {
            string[] results = new string[3];
            Stopwatch timer = new Stopwatch();
            timer.Start();

            List<City> bestRoute = new List<City>();
            double bestCost = double.MaxValue;
            for (int i = 0; i < Cities.Length; i++)
            {
                List<City> tempRoute = new List<City>();
                tempRoute.Add(Cities[i]);
                while (tempRoute.Count < Cities.Length) // n
                {
                    City city = tempRoute[tempRoute.Count - 1];
                    double min = double.MaxValue;
                    City closest = null;
                    foreach (City neighbor in Cities) // n
                    {
                        double dist = city.costToGetTo(neighbor);
                        if (dist < min && tempRoute.IndexOf(neighbor) < 0)
                        {
                            min = dist;
                            closest = neighbor;
                        }
                    }
                    tempRoute.Add(closest);
                }
                bssf = new TSPSolution(new ArrayList(tempRoute));
                double tempCost = costOfBssf();
                if (tempCost < bestCost)
                {
                    bestCost = tempCost;
                    bestRoute = tempRoute;
                }
            }

            timer.Stop();
            bssf = new TSPSolution(new ArrayList(bestRoute));
            results[COST] = costOfBssf().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = "1";
            return results;
        }

        // simulated annealing
        public string[] fancySolveProblem()
        {
            string[] results = new string[3];
            Stopwatch timer = new Stopwatch();
            int maxIter = (int)Math.Pow(Cities.Length, 3.5);
            timer.Start();

            defaultSolveProblem();
            List<City> solution = new List<City>(bssf.Route.ToArray(typeof(City)) as City[]);
            Random rand = new Random();
            int iterations = 0;
            while(timer.ElapsedMilliseconds < time_limit && iterations < maxIter)
            {
                iterations++;
                double heat = ((double)maxIter) / ((double)iterations);
                int a = rand.Next(solution.Count);
                int b = rand.Next(solution.Count);
                while (b == a)
                {
                    b = rand.Next(solution.Count);
                }
                List<City> neighbor = new List<City>(solution);
                City temp = neighbor[a];
                neighbor[a] = neighbor[b];
                neighbor[b] = temp;
                double prob = probability(solution, neighbor, heat);
                double hold = 0.5;
                if (prob >= hold)
                {
                    solution = neighbor;
                }
            }

            timer.Stop();
            bssf = new TSPSolution(new ArrayList(solution));
            results[COST] = costOfBssf().ToString();
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = "N/A";
            return results;
        }

        public double probability(List<City> solution, List<City> neighbor, double heat)
        {
            bssf = new TSPSolution(new ArrayList(solution));
            double cost = costOfBssf();
            bssf = new TSPSolution(new ArrayList(neighbor));
            double candidate = costOfBssf();
            if(candidate < cost)
            {
                return 1;
            }
            else
            {
                return Math.Pow(Math.E, (cost - candidate) / heat);
            }
        }

        // Held-Karp (good speed, bad memory)
        /*public string[] fancySolveProblem()
        {
            string[] results = new string[3];
            Stopwatch timer = new Stopwatch();
            int count = 0;
            timer.Start();

            double[][] costs = getCostMatrix();
            int n = Cities.Length;
            int npow = (int)Math.Pow(2, n);
            MultiDictionary C = new MultiDictionary();
            C.add(1, 0, 0);
            for(int s = 2; s <= n; s++) // go through all subset sizes
            {
                for(int S = 3; S < npow; S+=2) // go through all subsets with city 0
                {
                    if(numberOfSetBits(S) == s) // |S| = s
                    {
                        C.add(S, 0, double.MaxValue);
                        for(int j = 1; j < n; j++) // go through each city (exclude 0)
                        {
                            int jbit = (int)Math.Pow(2, j); // get the bit for the city
                            if((S & jbit) == jbit) // if this city is in the subset
                            {
                                double min = double.MaxValue;
                                for(int i = 0; i < n; i++) // go through each city (exclude 0)
                                {
                                    if(i != j)
                                    {
                                        int ibit = (int)Math.Pow(2, i); // get the bit for the city
                                        if((S & ibit) == ibit) // if this city is in the subset
                                        {
                                            int withoutj = (S & (~jbit));
                                            double cost = C.get(withoutj, i) + costs[i][j];
                                            if(cost < 0)
                                            {
                                                cost = double.MaxValue;
                                            }
                                            min = (cost < min) ? cost : min;
                                        }
                                    }
                                }
                                
                                C.add(S, j, min);
                            }
                        }
                    }
                }
            }

            int tour = npow - 1;
            double totalCost = double.MaxValue;
            ArrayList path = new ArrayList();
            int end = 0;
            while (tour != 1)
            {
                double min = double.MaxValue;
                int newEnd = n + 1;
                for(int j = 1; j < n; j++)
                {
                    int jbit = (int)Math.Pow(2, j);
                    if((tour & jbit) == jbit)
                    {

                        double cost = C.get(tour, j) + costs[j][end];
                        if(cost < min)
                        {
                            min = cost;
                            newEnd = j;
                            if(tour == npow - 1)
                            {
                                totalCost = cost;
                            }
                        }

                    }
                }
                end = newEnd;
                int endBit = (int)Math.Pow(2, end);
                tour = (tour & (~endBit));
                path.Add(Cities[end]);
            }
            path.Add(Cities[0]);
            path.Reverse();
            count = 1;

            timer.Stop();
            bssf = new TSPSolution(path);
            results[COST] = totalCost.ToString();
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();
            return results;
        }

        int numberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public double[][] getCostMatrix()
        {
            double[][] costs = new double[Cities.Length][];
            for(int from = 0; from < Cities.Length; from++)
            {
                costs[from] = new double[Cities.Length];
                for(int to = 0; to < Cities.Length; to++)
                {
                    costs[from][to] = (from == to) ? double.MaxValue : Cities[from].costToGetTo(Cities[to]);
                }
            }
            return costs;
        }*/
        #endregion
    }

}
