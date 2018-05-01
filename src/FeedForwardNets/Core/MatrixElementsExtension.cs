using System;
using System.Collections.Generic;
using System.Text;

namespace FeedForward.Core
{
    public partial class Matrix
    {
        /// <summary>
        ///     Квадратная подматрица со стороной нечетного размера, задаваемая центром и шириной.
        ///     Если часть подматрицы, или вся, вылазит за пределы матрицы, то эта часть заполняется
        ///     нулями.
        /// </summary>
        /// <param name="centerX"> Координата центра по оси Х </param>
        /// <param name="centerY"> Координата центра по оси У </param>
        /// <param name="width"> Ширина подматрицы (нечетное число) </param>
        /// <returns> Квадратная подматрица </returns>
        public Matrix GetSquareSlice(int centerX, int centerY, int width)
        {
            if (width % 2 != 1)
            {
                throw new ArgumentException();
            }

            var result = new Matrix(rows: width, columns: width, initializer: 0.0);

            var half = width / 2;

            // глобальные координаты левого верхнего угла квадрата
            var dx = centerX - half;
            var dy = centerY - half;
            for (int x = centerX - half; x <= centerX + half; x++)
            {
                for (int y = centerY - half; y <= centerY + half; y++)
                {
                    if (IsInRealBounds(x, y))
                    {
                        result[y - dy, x - dx] = this[y, x];
                    }
                }
            }

            return result;
        }

        public Matrix GetSubMatrix(int sliceFromInclusiveX, int sliceToExclusiveX, int sliceFromInclusiveY, int sliceToExclusiveY, bool useOriginal = true)
        {
            return new Matrix(data, sliceFromInclusiveX, sliceToExclusiveX, sliceFromInclusiveY, sliceToExclusiveY, useOriginal);
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

        public void ApplyElementwiseFunction(Func<int, int, double> func)
        {
            for (int i = allowedFromInclusiveY; i < allowedToExclusiveY; i++)
            {
                for (int j = allowedFromInclusiveX; j < allowedToExclusiveX; j++)
                {
                    data[i][j] = func(i, j);
                }
            }
        }


    }
}
