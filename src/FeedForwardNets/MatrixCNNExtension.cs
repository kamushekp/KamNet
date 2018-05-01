using FeedForward.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeedForward
{
    public static class MatrixCNNExtension
    {
        public static Matrix KernelConvolution(this Matrix matrix, Matrix kernel, bool zeroPaddedEdges = false)
        {
            if (kernel.Height != kernel.Width || kernel.Width % 2 != 1) throw new ArgumentException();

            var kernelSize = kernel.Width;

            var result = matrix.ReplaceElementsWithFuncOfElemsAround(x => x.Convolve(kernel), kernelSize, zeroPaddedEdges);

            return result;
        }

        public static Matrix Average2x2Pooling(this Matrix matrix)
        {
            var n = matrix.Height;
            var m = matrix.Width;

            if (n % 2 != 0 || m % 2 != 0) throw new ArgumentException();

            var result = new Matrix(n / 2, m / 2);

            for (int i = 1; i <= n / 2; i++)
            {
                for (int j = 1; j <= m / 2; j++)
                {
                    result[i - 1, j - 1] = Get2x2Average(matrix, i, j);
                }
            }

            return result;
        }

        private static double Get2x2Average(Matrix matrix, int y, int x)
        {
            var sum = 0.0;

            for (int u = 0; u <= 1; u++)
            {
                for (int v = 0; v <= 1; v++)
                {
                    sum += matrix[2 * y - u - 1, 2 * x - v - 1];
                }
            }
            return sum / 4.0;
        }

        public static double Convolve(this Matrix first, Matrix second)
        {
            if (first.Width != second.Width || first.Height != second.Height)
            {
                throw new NotImplementedException();
            }

            var sum = first.GetElements().Zip(second.GetElements(), (x, y) => x * y).Sum();
            return sum;
        }

        public static Matrix Get180Rotated(this Matrix matrix)
        {
            var n = matrix.Height;
            var m = matrix.Width;
            var rotated = new Matrix(n, m);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    rotated[n - i - 1, m - j - 1] = matrix[i, j];
                }
            }

            return rotated;
        }

        private static Matrix ReplaceElementsWithFuncOfElemsAround(this Matrix matrix, Func<Matrix, double> func, int windowSize, bool zeroPaddedEdges = false)
        {
            var half = windowSize / 2;

            Matrix result;
            Func<int, int> xIndex, yIndex;
            int xFrom, xTo, yFrom, yTo;

            if (zeroPaddedEdges)
            {
                result = new Matrix(matrix.Height, matrix.Width);

                xIndex = x => x;
                yIndex = y => y;

                xFrom = yFrom = 0;
                xTo = matrix.Width;
                yTo = matrix.Height;
            }
            else
            {
                result = new Matrix(matrix.Height - windowSize + 1, matrix.Width - windowSize + 1);

                xIndex = x => x - half;
                yIndex = y => y - half;

                xFrom = yFrom = half;
                xTo = matrix.Width - half;
                yTo = matrix.Height - half;
            }

            for (int y = yFrom; y < yTo; y++)
            {
                for (int x = xFrom; x < xTo; x++)
                {
                    var subMatrix = matrix.GetSquareSlice(x, y, windowSize);

                    result[yIndex(y), xIndex(x)] = func(subMatrix);
                }
            }

            return result;
        }
    }
}
