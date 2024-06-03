using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Diplom_Bacteria
{
    public partial class Form1 : Form
    {
        private Random random = new Random();
        private string filePath1 = @"C:\Users\kasya\source\repos\Diplom_Bacteria\Diplom_Bacteria\DataAnalit.txt"; private string filePath = @"C:\Users\kasya\source\repos\Diplom_Bacteria\Diplom_Bacteria\DataAnalit.txt";
        private string filePath2 = @"C:\Users\kasya\source\repos\Diplom_Bacteria\Diplom_Bacteria\Weights.txt";

        private StreamWriter writer; // Declare StreamWriter at class level

        private const int Rows = 50;
        private const int Columns = 50;

        private const int Iterations = 200;
        private int Iteration = 0;

        private const int PercentOfBacteria = 10;
        private const int PercentOfSugar = 40;


        private int NumBactriaCells = (int)(Rows * Columns * PercentOfBacteria / 100);
        int numberOfBacteria = (int)((PercentOfBacteria / 100.0) * (Rows * Columns));

        private int NumSugarCells = (int)(Rows * Columns * PercentOfSugar / 100);

        private const int Scale = 10;
        private const int Offset = 2;
        private int[,] gridArray = new int[Rows, Columns];
        private double[,] gridFood = new double[Rows, Columns];

       
        private int bacteria_min_neibors = 1;
        private int sugar_min_neibors = 2;
      
        private const int BirthLimitSugar = 2;
        private List<Tuple<int, int>> savedCoordinates = new List<Tuple<int, int>>();
        private double TresholdCatch = 0.5;

 
 
        private double[] S = new double[Iterations];
        private double[] X = new double[Iterations];

        private double[] S_reactor = new double[Iterations];
        private double[] X_reactor = new double[Iterations];

        private const int alpha = 2;
        private const int Mm = 400;
        private const int Km = 50;

        //private double b = 0.33;
        private double b = 0.5;
        private const int Smax = 200;
        // Define constants for grid cell types
        const int EmptyCell = 0;
        const int BacteriaCell = 1;
        const int NutrientCell = 2;

        // Define thresholds and parameters for the model
        const int NutrientThreshold = 2; // Threshold for nutrient growth
        const int BacteriaThreshold = 2; // Threshold for bacteria growth

        int caughtBacteria = 0;
        int sugar_cells = 0;
    

   
        const int maxGenerations = 2000;
   
        private int generation = 0;
        private int iteration = 0;
        double[] fitnes = new double[maxGenerations];


 

    
        double[] array1 = null;
        double[] array2 = null;

        public Form1()
        {
            InitializeComponent();
            writer = new StreamWriter(filePath, false);
          

            FillArray();
            // Assuming each set of values is separated by an empty line
         

            // Now you have setsOfValues array containing arrays of values
            // Convert these arrays of strings to arrays of doubles if needed
     

            S[0] = NumSugarCells;
            X[0] = NumBactriaCells;
            Random random = new Random();


        }

        private void Form1_Load(object sender, EventArgs e)
        {


            textBox11.Text = sugar_min_neibors.ToString();
            textBox12.Text = bacteria_min_neibors.ToString();
            textBox13.Text = TresholdCatch.ToString();
            textBox14.Text = Smax.ToString();
            timer1.Start();

        }
        static string[][] GetSetsOfValues(string[] lines)
        {
            var setsOfValues = new System.Collections.Generic.List<string[]>();
            var currentSet = new System.Collections.Generic.List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Add current set to the list and start a new set
                    setsOfValues.Add(currentSet.ToArray());
                    currentSet.Clear();
                }
                else
                {
                    // Add line to the current set
                    currentSet.Add(line);
                }
            }

            // Add the last set
            if (currentSet.Count > 0)
            {
                setsOfValues.Add(currentSet.ToArray());
            }

            return setsOfValues.ToArray();
        }
        private void FillArray()
        {
            Random random = new Random();
            int NumBactriaCells_fill = (int)(Rows * Columns * PercentOfBacteria / 100);
            int NumSugarCells_fill = (int)(Rows * Columns * PercentOfSugar / 100);
            while (NumBactriaCells_fill != 0 || NumSugarCells_fill != 0)
            {
                int x_cells = random.Next(1, Rows);
                int y_cells = random.Next(1, Columns);

                if (gridArray[x_cells, y_cells] == 0)
                {
                    if (NumBactriaCells_fill > 0)
                    {
                        gridArray[x_cells, y_cells] = 1;
                        NumBactriaCells_fill--;
                    }
                    else if (NumSugarCells_fill > 0)
                    {
                        gridArray[x_cells, y_cells] = 2;
                        gridFood[x_cells, y_cells] = 1.0;
                        NumSugarCells_fill--;
                    }
                }
            }
        }


 


        private int CatchBacteria(double collectionProbability)
        {
            Random random = new Random(); // Make sure to initialize a new Random instance
            int catch_bact = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)

                {
                    if (gridArray[i, j] == 1)
                    {
                        // Выбор случайного индекса
                        double randomValueCatch = random.NextDouble();

                        if (randomValueCatch < collectionProbability)
                        {
                            gridArray[i, j] = 0;
                            catch_bact++;
                        }
                    }




                }
            }
            return catch_bact;
        }


        private int StartSugar(double feedingProbability)
        {
            Random random = new Random(); // Make sure to initialize a new Random instance
            int catch_bact = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)

                {
                    if (gridArray[i, j] == 0)
                    {
                        // Выбор случайного индекса
                        double randomValueCatch = random.NextDouble();

                        if (randomValueCatch >= feedingProbability)
                        {
                            gridArray[i, j] = 2;
                            catch_bact++;
                        }
                    }




                }
            }
            return catch_bact;
        }

        private int StartSubstrat(double feedingProbability)
        {
            var res = Smax * feedingProbability;
            int substratAdded = Convert.ToInt32(res);


            for (int k = 0; k < substratAdded; k++)
            {

                int x_new = random.Next(0, Rows);
                int y_new = random.Next(0, Columns);
                gridArray[x_new, y_new] = 2;

            }

            return substratAdded;
        }




        // Function to count the number of neighboring cells of a specific type
        int CountNeighborsOfType(int[,] grid, int x, int y, int type)
        {
            int rows = grid.GetLength(0);
            int columns = grid.GetLength(1);

            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < rows && ny >= 0 && ny < columns && !(dx == 0 && dy == 0))
                    {
                        if (grid[nx, ny] == type)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }


        void ReproduceBacteria(int[,] gridArray, int x, int y)
        {
            // Define the possible directions for reproduction (assuming 4 neighbors: up, down, left, right)
            int[,] directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

            // Iterate through each direction
            for (int d = 0; d < directions.GetLength(0); d++)
            {
                // Calculate the new position for reproduction
                int newX = x + directions[d, 0];
                int newY = y + directions[d, 1];

                // Check if the new position is within the grid bounds and is an empty cell
                if (IsValidPosition(gridArray, newX, newY) && gridArray[newX, newY] == 0)
                {
                    // Spawn a new bacteria in the empty cell
                    gridArray[newX, newY] = 1;
                    NumBactriaCells += 1;
                    // Assuming only one reproduction per turn, so break the loop
                    break;
                }
            }
        }
        private int GetRandomNeighbor(int i, int j)
        {
            int[] directions = { -1, 0, 1 };
            int randomDirection = directions[new Random().Next(3)];
            return i + randomDirection;
        }
        bool IsValidPosition(int[,] gridArray, int x, int y)
        {
            // Check if the position (x, y) is within the grid bounds
            return x >= 0 && x < gridArray.GetLength(0) && y >= 0 && y < gridArray.GetLength(1);
        }

        private void Updating()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int nutrientNeighbors = CountNeighborsOfType(gridArray, i, j, 2);
                    int bacteriaNeighbors = CountNeighborsOfType(gridArray, i, j, 1);

                    // Apply the rules to the current cell
                    if (gridArray[i, j] == 0) // Empty cell
                    {
                        // If there are enough nutrient neighbors, create a new bacteria cell
                        if (nutrientNeighbors > 0 && bacteriaNeighbors > 1)
                        {
                            gridArray[i, j] = 1; // Bacteria cell

                            NumBactriaCells++;
                        }
                    }
                    else if (gridArray[i, j] == 1) // Bacteria cell
                    {
                        // If there are enough nutrient neighbors, grow and divide
                        if (nutrientNeighbors >= 1)
                        {
                            gridArray[i, j] = 1; // Bacteria cell (grow)
                            ReproduceBacteria(gridArray, i, j);                     // Create a new bacteria cell in a neighboring empty cell

                        }

                    }
                    else if (gridArray[i, j] == 2) // Nutrient cell
                    {
                        // If there are enough bacteria neighbors, consume nutrient
                        if (bacteriaNeighbors >= 1)
                        {
                            ReproduceBacteria(gridArray, i, j);
                            gridArray[i, j] = 0; // Empty cell
                            NumSugarCells--;
                        }
                    }

                }
            }
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            Graphics g = e.Graphics;


            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Columns; y++)
                {
                    int yPos = y * Scale;
                    int xPos = x * Scale;
                    Color color = Color.FromArgb(255, 255, 255);
                    if (gridArray[x, y] == 2) //синий
                    {
                        color = Color.FromArgb(0, 0, 255);
                    }

                    else if (gridArray[x, y] == 1) //красный
                    {
                        color = Color.FromArgb(255, 0, 0);
                    }

                    g.FillRectangle(new SolidBrush(color), xPos, yPos, Scale - Offset, Scale - Offset);
                }
            }
        }

        // Fitness Function (example placeholder)
        private double FitnessFunction(double[,] chromosomes, double[,] chromosomes2, int index)
        {
            // You need to implement your own fitness function based on your problem
            // For demonstration purposes, let's just sum up values in a chromosome
            double fitness1 = 0;
            double fitness2 = 0;
            for (int i = 0; i < Iteration; i++)
            {
                fitness2 += chromosomes2[index, i];
                fitness1 += chromosomes[index, i];
            }
            return fitness2 - fitness1;
        }


        // Tournament Selection
        private int TournamentSelection(double[,] chromosomes, double[,] chromosomes2, int tournamentSize)
        {
            Random random = new Random();
            int bestIndex = random.Next(chromosomes.GetLength(0)); // Initialize best index randomly
            double bestFitness = double.MinValue;

            for (int i = 1; i < tournamentSize; i++)
            {
                int randomIndex = random.Next(chromosomes.GetLength(0));
                double fitness = FitnessFunction(chromosomes, chromosomes2, randomIndex);
                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestIndex = randomIndex;
                }
            }

            return bestIndex;
        }

        // Function to perform crossover between two parent chromosomes
        private double[] Crossover(double[] parent1, double[] parent2)
        {
            Random random = new Random();
            int crossoverPoint = random.Next(0, parent1.Length); // Select a random crossover point

            double[] offspring = new double[parent1.Length];

            // Copy genetic information from parents to offspring with crossover
            for (int i = 0; i < parent1.Length; i++)
            {
                //for (int j = 0; j < parent1.GetLength(1); j++)

                if (i < crossoverPoint)
                {
                    offspring[i] = parent1[i]; // Copy genetic information from parent 1
                }
                else
                {
                    offspring[i] = parent2[i]; // Copy genetic information from parent 2
                }

            }

            return offspring;
        }
        // Function to extract chromosome from 2D array based on index
        private double[] GetChromosomeFromIndex(double[,] chromosomeArray, int index)
        {
            //int numGenerations = chromosomeArray.GetLength(1);
            double[] chromosome = new double[Iterations];
            for (int i = 0; i < Iterations; i++)
            {
                //for (int j = 0; j < numGenerations; j++)              
                chromosome[i] = chromosomeArray[index, i];

            }
            return chromosome;
        }

        // Function to replace chromosome at given index
        private void ReplaceChromosomeAtIndex(double[,] chromosomeArray, int index, double[] newChromosome)
        {
            int numGenerations = newChromosome.Length;
            for (int j = 0; j < numGenerations; j++)
            {
                chromosomeArray[index, j] = newChromosome[j];
            }
        }




        // Function to perform mutation on a chromosome
        private double[] Mutate(double[] chromosome, double mutationRate)
        {
            Random random = new Random();

            for (int i = 0; i < chromosome.Length; i++)
            {
                //for (int j = 0; j < chromosome.GetLength(1); j++)

                if (random.NextDouble() < mutationRate)
                {
                    // Mutate the gene at this position by adding a small random value
                    chromosome[i] += random.NextDouble() * 0.1 - 0.05; // Mutation range (-0.05 to 0.05)

                    // Ensure the mutated gene remains within the valid range (0 to 1)
                    chromosome[i] = Math.Max(0, Math.Min(1, chromosome[i]));
                }

            }

            return chromosome;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            var diff = caughtBacteria - sugar_cells;




            textBox3.Text = iteration.ToString();
            textBox4.Text = generation.ToString();
            if (iteration < Iterations)
            {



                writer.WriteLine($"{caughtBacteria} {sugar_cells}");
                Updating();

        


                Random random = new Random();

              
                sugar_cells = StartSubstrat(2*random.NextDouble());
                NumSugarCells += sugar_cells;


                caughtBacteria = CatchBacteria(random.NextDouble());
                NumBactriaCells = NumBactriaCells-caughtBacteria;



                //else if (iteration > 20 && iteration <= 30)
                //{
                //    sugar_cells = 0;
                //}
                //else if (iteration > 30 && iteration < 80)
                //{
                //    sugar_cells = StartSugar(0.5);
                //    NumSugarCells += sugar_cells;

                //}
                //else if (iteration > 80 && iteration <= 90)
                //{
                //    sugar_cells = StartSugar(1);
                //    NumSugarCells += sugar_cells;
                //}
                //else if (iteration > 90 && iteration <= 190)
                //{
                //    sugar_cells = StartSugar(2);
                //    NumSugarCells += sugar_cells;
                //}
                //else
                //{
                //    sugar_cells = StartSugar(0);
                //    NumSugarCells += sugar_cells;

                //}
                //if (NumBactriaCells > 10)
                //{

                //    caughtBacteria = CatchBacteria(0.5);
                //    NumBactriaCells -= caughtBacteria;
                //}

                //sugar_cells = StartSubstrat(1);
                //NumSugarCells += sugar_cells;
                //caughtBacteria = CatchBacteria(0.5);
                //NumBactriaCells -= caughtBacteria;

                Invalidate();
                iteration++;




            }


            else
            {

                timer1.Stop();
                writer.Close();



            }
        }


     










    }
}

