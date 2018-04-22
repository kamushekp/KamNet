using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KamNet
{
    public class Matrix
    {
        /// <summary>
        ///     массив с данными
        /// </summary>
        private double[][] data;

        /// <summary>
        ///     ограничительные индексы, в рамках которых идет итерация внутри массива с данными.
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
        ///     индексация явно с 0 до высоты/ширины, но неявно - с учетом ограничительных индексов
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
                    throw new ArgumentNullException();
                }

                return data[y + allowedFromInclusiveY][x + allowedFromInclusiveX];
            }

            set
            {
                if (IsInBounds(x, y))
                {
                    throw new ArgumentNullException();
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
            if (AreRowsNotSameSize(data))
            {
                throw new ArgumentException();
            }

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
            }
            else
            {
                SetData(CopyData(data));
            }

            if (sliceFromInclusiveX < 0 ||
                sliceFromInclusiveY < 0 ||
                sliceToExclusiveX > dataWidth ||
                sliceToExclusiveY > dataHeight)
            {
                throw new ArgumentException();
            }

            SetIndexing(sliceFromInclusiveX, sliceFromInclusiveY, sliceToExclusiveX, sliceToExclusiveY);
        }

        public Matrix GetSubMatrix(int sliceFromInclusiveX, int sliceToExclusiveX, int sliceFromInclusiveY, int sliceToExclusiveY, bool useOriginal = true)
        {
            return new Matrix(data, sliceFromInclusiveX, sliceFromInclusiveY, sliceToExclusiveX, sliceToExclusiveY, useOriginal);
        }

        public IEnumerable<double> GetElements()
        {
            for (int i = allowedFromInclusiveY; i < allowedToExclusiveY; i++)
            {
                for (int j = allowedFromInclusiveX; j < allowedToExclusiveX; j++)
                {
                    yield return this.data[i][j];
                }
            }
        }

        public void ApplyElementwiseFunction(Func<double, double> func)
        {
            for (int i = allowedFromInclusiveY; i < allowedToExclusiveY; i++)
            {
                for (int j = allowedFromInclusiveX; j < allowedToExclusiveX; j++)
                {
                    data[i][j] = func(data[i][j]);
                }
            }
        }

        public void Add(Matrix matrix)
        {
            for (int i = allowedFromInclusiveY; i < allowedToExclusiveY; i++)
            {
                for (int j = allowedFromInclusiveX; j < allowedToExclusiveX; j++)
                {
                    data[i][j] += matrix.data[i][j];
                }
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

        private bool AreShapesSame(Matrix first, Matrix second)
        {
            var areSame = first.allowedFromInclusiveX == second.allowedFromInclusiveX &&
                first.allowedFromInclusiveY == second.allowedFromInclusiveY &&
                first.allowedToExclusiveX == second.allowedToExclusiveX &&
                first.allowedToExclusiveY == second.allowedToExclusiveY;

            return areSame;
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
