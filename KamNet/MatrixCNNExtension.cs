using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KamNet
{
    public static class MatrixCNNExtension
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


        public static double Convolve(this Matrix first, Matrix second)
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
    }
}
