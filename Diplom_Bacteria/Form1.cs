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
        // Создание экземпляра Random для генерации случайных чисел
        private Random random = new Random();

        // Объявление StreamWriter на уровне класса для записи данных в файл
        private StreamWriter writer;

        // Константы для размеров сетки
        private const int Rows = 50;
        private const int Columns = 50;

        // Константа для количества итераций
        private const int Iterations = 200;
      

        // Константы для процентов бактерий и сахара
        private const int PercentOfBacteria = 10;
        private const int PercentOfSugar = 40;

        // Вычисление количества клеток с бактериями и сахаром
        private int NumBactriaCells = (int)(Rows * Columns * PercentOfBacteria / 100);
        private int NumSugarCells = (int)(Rows * Columns * PercentOfSugar / 100);

        // Константы для масштаба и смещения
        private const int Scale = 10;
        private const int Offset = 2;
        private int[,] gridArray = new int[Rows, Columns];
        private double[,] gridFood = new double[Rows, Columns];

        // Минимальное количество соседей для бактерий и сахара
        private int bacteria_min_neibors = 1;
        private int sugar_min_neibors = 2;

        // Порог для создания сахара
        private const int BirthLimitSugar = 2;
        private List<Tuple<int, int>> savedCoordinates = new List<Tuple<int, int>>();
        private double TresholdCatch = 0.5;

        // Массивы для хранения данных по итерациям
        private double[] S = new double[Iterations];
        private double[] X = new double[Iterations];


        // Максимальное количество сахара
        private const int Smax = 200;

        // Определение типов клеток
        const int EmptyCell = 0;
        const int BacteriaCell = 1;
        const int NutrientCell = 2;

        // Пороги для роста бактерий и сахара
        const int NutrientThreshold = 2;
        const int BacteriaThreshold = 2;

        int caughtBacteria = 0;
        int sugar_cells = 0;

        // Переменные для отслеживания поколений и итераций
        private int generation = 0;
        private int iteration = 0;

        public Form1()
        {
            InitializeComponent();
            // Получение пути директории, где находится исполняемый файл
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Формирование полного пути к файлу для записи данных
            string filePath = Path.Combine(exeDirectory, "DataAnalit.txt");
            writer = new StreamWriter(filePath, false);

            // Заполнение массива начальных значений
            FillArray();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Инициализация текстовых полей значениями параметров
            textBox11.Text = sugar_min_neibors.ToString();
            textBox12.Text = bacteria_min_neibors.ToString();
            textBox13.Text = TresholdCatch.ToString();
            textBox14.Text = Smax.ToString();
            timer1.Start();
        }

        // Метод для заполнения массива начальных значений
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

        // Метод для поимки бактерий с заданной вероятностью
        private int CatchBacteria(double collectionProbability)
        {
            Random random = new Random();
            int catch_bact = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (gridArray[i, j] == 1)
                    {
                        // Генерация случайного значения и сравнение с вероятностью
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

        // Метод для добавления сахара в случайные клетки
        private int StartSugar(double feedingProbability)
        {
            Random random = new Random();
            int catch_bact = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (gridArray[i, j] == 0)
                    {
                        // Генерация случайного значения и сравнение с вероятностью
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

        // Метод для добавления субстрата в случайные клетки
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

        // Функция для подсчета соседей определенного типа
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

        // Метод для размножения бактерий
        void ReproduceBacteria(int[,] gridArray, int x, int y)
        {
            // Возможные направления для размножения
            int[,] directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

            // Проверка каждого направления
            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int newX = x + directions[d, 0];
                int newY = y + directions[d, 1];

                // Проверка, что новое положение в пределах сетки и является пустой клеткой
                if (IsValidPosition(gridArray, newX, newY) && gridArray[newX, newY] == 0)
                {
                    gridArray[newX, newY] = 1;
                    NumBactriaCells += 1;
                    break;
                }
            }
        }

        // Метод для получения случайного соседнего индекса
        private int GetRandomNeighbor(int i, int j)
        {
            int[] directions = { -1, 0, 1 };
            int randomDirection = directions[new Random().Next(3)];
            return i + randomDirection;
        }

        // Метод для проверки валидности позиции в сетке
        bool IsValidPosition(int[,] gridArray, int x, int y)
        {
            return x >= 0 && x < gridArray.GetLength(0) && y >= 0 && y < gridArray.GetLength(1);
        }

        // Метод для обновления состояния сетки
        private void Updating()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int nutrientNeighbors = CountNeighborsOfType(gridArray, i, j, 2);
                    int bacteriaNeighbors = CountNeighborsOfType(gridArray, i, j, 1);

                    // Применение правил к текущей клетке
                    if (gridArray[i, j] == 0) // Пустая клетка
                    {
                        if (nutrientNeighbors > 0 && bacteriaNeighbors > 1)
                        {
                            gridArray[i, j] = 1; // Бактериальная клетка
                            NumBactriaCells++;
                        }
                    }
                    else if (gridArray[i, j] == 1) // Бактериальная клетка
                    {
                        if (nutrientNeighbors >= 1)
                        {
                            gridArray[i, j] = 1; // Бактериальная клетка (рост)
                            ReproduceBacteria(gridArray, i, j); // Размножение бактерий
                        }
                    }
                    else if (gridArray[i, j] == 2) // Клетка с питательными веществами
                    {
                        if (bacteriaNeighbors >= 1)
                        {
                            ReproduceBacteria(gridArray, i, j);
                            gridArray[i, j] = 0; // Пустая клетка
                            NumSugarCells--;
                        }
                    }
                }
            }
        }

        // Метод для отрисовки сетки на форме
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
                    if (gridArray[x, y] == 2) // Клетка с питательными веществами (синий цвет)
                    {
                        color = Color.FromArgb(0, 0, 255);
                    }
                    else if (gridArray[x, y] == 1) // Бактериальная клетка (красный цвет)
                    {
                        color = Color.FromArgb(255, 0, 0);
                    }

                    g.FillRectangle(new SolidBrush(color), xPos, yPos, Scale - Offset, Scale - Offset);
                }
            }
        }

        // Метод для обработки событий таймера
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

                // Добавление субстрата и поимка бактерий
                sugar_cells = StartSubstrat(2 * random.NextDouble());
                NumSugarCells += sugar_cells;
                caughtBacteria = CatchBacteria(random.NextDouble());
                NumBactriaCells -= caughtBacteria;

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
