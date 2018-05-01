using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedForward.Core
{
    public partial class Matrix
    {
        /// <summary>
        ///     массив с данными
        /// </summary>
        private double[][] data;
        
        /// <summary>
        ///     ограничительные индексы, в рамках которых идет итерация по внутриннему массиву с данными.
        ///     Нужно для того, чтобы можно было создавать слайсы матрицы без копирования данных.
        /// </summary>
        private int allowedFromInclusiveX;
        private int allowedFromInclusiveY;

        private int allowedToExclusiveX;
        private int allowedToExclusiveY;

        /// <summary>
        ///     высота и ширина внутреннего массива с данными data[][]
        /// </summary>
        private int dataHeight;
        private int dataWidth;
        
        /// <summary>
        ///     параметры матрицы выражаются относительно ограничетельных индексов.
        /// </summary>
        public int Height
        {
            get
            {
                return allowedToExclusiveY - allowedFromInclusiveY;
            }
        }
        public int Width
        {
            get
            {
                return allowedToExclusiveX - allowedFromInclusiveX;
            }
        }

        /// <summary>
        ///     индексация явно с 0 до высоты/ширины, но неявно (вдруг это слайс) - с учетом ограничительных индексов
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double this[int y, int x]
        {
            get
            {
                if (!IsInBounds(x, y))
                {
                    throw new ArgumentException();
                }

                return data[y + allowedFromInclusiveY][x + allowedFromInclusiveX];
            }

            set
            {
                if (!IsInBounds(x, y))
                {
                    throw new ArgumentException();
                }

                data[y + allowedFromInclusiveY][x + allowedFromInclusiveX] = value;
            }
        }

        /// <summary>
        ///     обертка jagged массива матрицей
        /// </summary>
        /// <param name="data"></param>
        public Matrix(double[][] data)
        {
            if (IsDataEmpty(data) || AreRowsNotSameSize(data))
            {
                throw new ArgumentException();
            }

            SetData(data);

            SetDefaultIndexing();
        }

        /// <summary>
        ///     создание нового массива данных и матрицы вокруг
        /// </summary>
        /// <param name="rows"> количество строк</param>
        /// <param name="columns"> количество столбцов </param>
        /// <param name="initializer"> значение по умолчанию для элементов матрицы </param>
        public Matrix(int rows, int columns, double initializer = 0)
        {
            var newArray = (new double[rows][]).
                Select(x => (new double[columns]).Select(_ => initializer).ToArray()).
                ToArray();

            SetData(newArray);
            SetDefaultIndexing();
        }

        /// <summary>
        ///     Создание новой матрицы на основе матрицы
        /// </summary>
        /// <param name="matrix">   Матрица, содержимое которой будет скопировано </param>
        public Matrix(Matrix matrix)
        {
            SetData(CopyData(matrix.data));
            SetIndexing(matrix);
        }

        /// <summary>
        ///     создание новой матрицы на основе двух и поэлементной функции
        /// </summary>
        /// <param name="first"> первая матрица</param>
        /// 
        /// <param name="second"> вторая матрица </param>
        /// <param name="elementwiseFunc"> функция: new_i_j = func(first_i,j, second_i_j) </param>
        public Matrix(Matrix first, Matrix second, Func<double, double, double> elementwiseFunc)
        {
            if (!AreShapesSame(first, second))
            {
                throw new ArgumentException();
            }

            var result = new Matrix(first.Height, first.Width);

            for (int i = 0; i < first.Height; i++)
            {
                for (int j = 0; j < first.Width; j++)
                {
                    result[i, j] = 
                        elementwiseFunc(
                            first.data[first.allowedFromInclusiveX + i][first.allowedFromInclusiveY + j],
                            second.data[second.allowedFromInclusiveX + i][second.allowedFromInclusiveY + j]);
                }
            }

            SetData(result.data);
            SetDefaultIndexing();
        }

        /// <summary>
        ///     обертывание части исходного массива данных double[][] в объект типа "матрица"
        /// </summary>
        /// <param name="data"> обертываемый массив</param>
        /// <param name="sliceFromInclusiveX"> начальный индекс по столбцам, включительно </param>
        /// <param name="sliceToExclusiveX"> конечный индекс по столбцам, не включая </param>
        /// <param name="sliceFromInclusiveY"> начальный индекс по строкам, включительно </param>
        /// <param name="sliceToExclusiveY"> конечный индекс по строкам, не включая </param>
        /// <param name="useOriginal"> True = будет использована память передаваемого массива (по ссылке). False = данные будут скопированы (по значению) </param>
        public Matrix(double[][] data, int sliceFromInclusiveX, int sliceToExclusiveX, int sliceFromInclusiveY, int sliceToExclusiveY, bool useOriginal = true)
        {
            if (sliceFromInclusiveX >= sliceToExclusiveX ||
                sliceFromInclusiveY >= sliceToExclusiveY )
            {
                throw new ArgumentException();
            }

            if (AreRowsNotSameSize(data))
            {
                throw new ArgumentException();
            }

            if (useOriginal)
            {
                SetData(data);

                if (sliceFromInclusiveX < 0 ||
                    sliceFromInclusiveY < 0 ||
                    sliceToExclusiveX > dataWidth ||
                    sliceToExclusiveY > dataHeight)
                {
                    throw new ArgumentException();
                }

                SetIndexing(sliceFromInclusiveX, sliceFromInclusiveY, sliceToExclusiveX, sliceToExclusiveY);

            }
            else
            {
                SetData(CopyData(data, sliceFromInclusiveX, sliceFromInclusiveY, sliceToExclusiveX, sliceToExclusiveY));

                SetDefaultIndexing();
            }
        }

        public override bool Equals(object obj)
        {
            var another = obj as Matrix;
            if (another == null)
            {
                return false;
            }

            if (this.Height != another.Height || this.Width != another.Width)
            {
                return false;
            }

            var firstElems = this.GetElements().ToArray();
            var secondElems = another.GetElements().ToArray();

            for (int i = 0; i < firstElems.Length; i++)
            {
                if (firstElems[i] != secondElems[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            if (Height == dataHeight && Width == dataWidth)
            {
                return $"Matrix [{Height} x {Width}]";
            }
            else
            {
                return $"Submatrix [{Height} x {Width}] from [{dataHeight} x {dataWidth}]";
            }

        }

        public bool IsInBounds(int x, int y)
        {
            var isInBounds =
                y >= 0 &&
                x >= 0 &&
                x < Width &&
                y < Height;

            return isInBounds;
        }

        private void SetDefaultIndexing()
        {
            if (data != null)
            {
                allowedFromInclusiveX = 0;
                allowedFromInclusiveY = 0;

                allowedToExclusiveY = data.Length;
                if (data[0] != null)
                {
                    allowedToExclusiveX = data[0].Length;
                }
            }
        }

        private bool IsInRealBounds(int x, int y)
        {
            var inBounds =
                x >= allowedFromInclusiveX &&
                y >= allowedFromInclusiveY &&
                x < allowedToExclusiveX &&
                y < allowedToExclusiveY;

            return inBounds;

        }

        private bool AreShapesSame(Matrix first, Matrix second)
        {
            var areSame = first.allowedFromInclusiveX == second.allowedFromInclusiveX &&
                first.allowedFromInclusiveY == second.allowedFromInclusiveY &&
                first.allowedToExclusiveX == second.allowedToExclusiveX &&
                first.allowedToExclusiveY == second.allowedToExclusiveY;

            return areSame;
        }

        private double[][] CopyData(double[][] data, int sliceFromInclusiveX, int sliceFromInclusiveY, int sliceToExclusiveX, int sliceToExclusiveY)
        {
            var n = sliceToExclusiveY - sliceFromInclusiveY;
            var m = sliceToExclusiveX - sliceFromInclusiveX;

            var result = new double[n][];

            for (int row = 0; row < n; row++)
            {
                result[row] = new double[m];

                for (int col = 0; col < m; col++)
                {
                    result[row][col] = data[row + sliceFromInclusiveY][col + sliceFromInclusiveX];
                }
            }

            return result;
        }

        private double[][] CopyData(double[][] data)
        {
            return data.Select(row => row.Select(element => element).ToArray()).ToArray();
        }

        private void SetData(double[][] data)
        {
            this.data = data;
            this.dataHeight = data.Length;
            this.dataWidth = data[0].Length;
        }

        private void SetIndexing(int allowedFromInclusiveX, int allowedFromInclusiveY, int allowedToExclusiveX, int allowedToExclusiveY)
        {
            this.allowedFromInclusiveX = allowedFromInclusiveX;
            this.allowedFromInclusiveY = allowedFromInclusiveY;
            this.allowedToExclusiveX = allowedToExclusiveX;
            this.allowedToExclusiveY = allowedToExclusiveY;
        }

        private void SetIndexing(Matrix matrix)
        {
            this.allowedFromInclusiveX = matrix.allowedFromInclusiveX;
            this.allowedFromInclusiveY = matrix.allowedFromInclusiveY;
            this.allowedToExclusiveX = matrix.allowedToExclusiveX;
            this.allowedToExclusiveY = matrix.allowedToExclusiveY;
        }

        private bool AreRowsNotSameSize(double[][] data)
        {
            var lenghts = data.Select(x => x.Length);
            return !lenghts.All(x => x == lenghts.FirstOrDefault());
        }

        private bool IsDataEmpty(double[][] data)
        {
            if (data == null)
            {
                return true;
            }

            if (data[0] == null || data[0].Length == 0)
            {
                return true;
            }

            return false;
        }
    }
}
