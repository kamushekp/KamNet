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
        ///     параметры матрицы выражаются относительно ограничетельных индексов.
        /// </summary>
        public int Height { get { return data != null ? allowedToExclusiveY - allowedFromInclusiveY : 0; } }
        public int Width { get { return data != null ? (data[allowedFromInclusiveY] != null ? allowedToExclusiveX - allowedFromInclusiveX : 0) : 0; } }


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
            if (AreRowsNotSameSize(data))
            {
                throw new ArgumentException();
            }

            InitFromData(data);
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

            data = (new double[rows][]).
                Select(x => (new double[columns]).Select(_ => initializer).ToArray()).
                ToArray();

            InitFromData(data);
        }

        /// <summary>
        ///     Создание новой матрицы на основе матрицы
        /// </summary>
        /// <param name="matrix">   Матрица, содержимое которой будет скопировано </param>
        public Matrix(Matrix matrix)
        {
            this.data = matrix.data.Select(x => x.Select(e => e).ToArray()).ToArray();

            this.allowedFromInclusiveX = matrix.allowedFromInclusiveX;
            this.allowedFromInclusiveY = matrix.allowedFromInclusiveY;
            this.allowedToExclusiveX = matrix.allowedToExclusiveX;
            this.allowedToExclusiveY = matrix.allowedToExclusiveY;
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

            InitFromData(result.data);
        }

        /// <summary>
        ///     обертывание части исходного массива.
        /// </summary>
        /// <param name="data"> обертываемый массив</param>
        /// <param name="sliceFromInclusiveX"> начальный индекс по столбцам, включительно </param>
        /// <param name="sliceToExclusiveX"> конечный индекс по столбцам, не включая </param>
        /// <param name="sliceFromInclusiveY"> начальный индекс по строкам, включительно </param>
        /// <param name="sliceToExclusiveY"> конечный индекс по строкам, не включая </param>
        /// <param name="useOriginal"> True = будет использована память передаваемого массива (по ссылке). False = данные будут скопированы (по значению) </param>
        public Matrix(double[][] data, int sliceFromInclusiveX, int sliceToExclusiveX, int sliceFromInclusiveY, int sliceToExclusiveY, bool useOriginal = true)
        {
            if (sliceFromInclusiveX >= sliceToExclusiveX || sliceFromInclusiveY >= sliceToExclusiveY)
            {
                throw new ArgumentException();
            }

            if (AreRowsNotSameSize(data))
            {
                throw new ArgumentException();
            }

            if (useOriginal)
            {
                this.data = data;
            }
            else
            {
                this.data = GetCopiedMatrixData(data);
            }

            this.allowedFromInclusiveX = sliceFromInclusiveX;
            this.allowedFromInclusiveY = sliceFromInclusiveY;
            this.allowedToExclusiveX = sliceToExclusiveX;
            this.allowedToExclusiveY = sliceToExclusiveY;
        }

        public Matrix GetSubMatrix(int sliceFromInclusiveX, int sliceToExclusiveX, int sliceFromInclusiveY, int sliceToExclusiveY, bool useOriginal = true)
        {
            return new Matrix(data, sliceFromInclusiveX, sliceFromInclusiveY, sliceToExclusiveX, sliceToExclusiveY, useOriginal);
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

        private void InitFromData(double[][] data)
        {
            this.data = data;
            SetDefaultIndexing();
        }

        private bool AreShapesSame(Matrix first, Matrix second)
        {
            var areSame = first.allowedFromInclusiveX == second.allowedFromInclusiveX &&
                first.allowedFromInclusiveY == second.allowedFromInclusiveY &&
                first.allowedToExclusiveX == second.allowedToExclusiveX &&
                first.allowedToExclusiveY == second.allowedToExclusiveY;

            return areSame;
        }

        private double[][] GetCopiedMatrixData(double[][] data)
        {
            return data.Select(row => row.Select(element => element).ToArray()).ToArray();
        }

        private bool AreRowsNotSameSize(double[][] data)
        {
            var lenghts = data.Select(x => x.Length);
            return !lenghts.All(x => x == lenghts.FirstOrDefault());
        }
    }

    public static class MatrixExtensions
    {
        public static Matrix KernelPooling(this Matrix matrix, Matrix kernel, bool zeroPaddedEdges = false)
        {
            if (kernel.Height != kernel.Width || kernel.Width % 2 != 1) throw new NotImplementedException();

            var kernelSize = kernel.Width;
            var half = kernelSize / 2;

            Matrix result;
            Func<int, int> xIndex, yIndex;
            if (zeroPaddedEdges)
            {
                result = new Matrix(matrix.Height, matrix.Width);
                xIndex = x => x;
                yIndex = y => y;
            }
            else
            {
                result = new Matrix(matrix.Height - kernelSize + 1, matrix.Width - kernelSize + 1);
                xIndex = x => x - half;
                yIndex = y => y - half;
            }

            for (int y = half; y < result.Height - half; y++)
            {
                for (int x = half; x < result.Width - half; x++)
                {
                    var subMatrix = matrix.GetSubMatrix(x, x + kernelSize, y, y + kernelSize);

                    result[yIndex(y), xIndex(x)] = Convolve(subMatrix, kernel);
                }
            }

            return result;
        }

        public static Matrix MaxPooling(this Matrix matrix, int windowSize, bool zeroPaddedEdges = false)
        {
            if (windowSize % 2 != 1) throw new ArgumentException();

            if (zeroPaddedEdges) throw new NotImplementedException();

            var half = windowSize / 2;

            Matrix result;
            Func<int, int> xIndex, yIndex;
            if (zeroPaddedEdges)
            {
                result = new Matrix(matrix.Height, matrix.Width);
                xIndex = x => x;
                yIndex = y => y;
            }
            else
            {
                result = new Matrix(matrix.Height - windowSize + 1, matrix.Width - windowSize + 1);
                xIndex = x => x - half;
                yIndex = y => y - half;
            }

            for (int y = half; y < result.Height - half; y++)
            {
                for (int x = half; x < result.Width - half; x++)
                {
                    var subMatrix = matrix.GetSubMatrix(x, x + windowSize, y, y + windowSize);

                    result[yIndex(y), xIndex(x)] = subMatrix.GetElements().Max();
                }
            }

            return result;
        }


        private static double Convolve(Matrix first, Matrix second)
        {
            if (first.Width != second.Width || first.Height != second.Height)
            {
                throw new NotImplementedException();
            }
            
            var sum = 0.0;

            for (int i = 0; i < first.Height; i++)
            {
                for (int j = 0; j < first.Width; j++)
                {
                    if (first.IsInBounds(i, j) && second.IsInBounds(i, j))
                    {
                        sum += first[i, j] * second[i, j];
                    }
                }
            }

            return sum;
        }

        public static IEnumerable<double> GetElements(this Matrix matrix)
        {
            for(int i = 0; i < matrix.Height; i++)
            {
                for (int j = 0; j < matrix.Width; j++)
                {
                    yield return matrix[i, j];
                }
            }
        }
    }
}
