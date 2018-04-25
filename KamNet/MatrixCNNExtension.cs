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

            var result = matrix.ReplaceElementsWithFuncOfElemsAround(x => x.Convolve(kernel), kernelSize, zeroPaddedEdges);

            return result;
        }

        public static Matrix MaxPooling(this Matrix matrix, int windowSize, bool zeroPaddedEdges = false)
        {
            if (windowSize % 2 != 1) throw new NotImplementedException();

            var result = matrix.ReplaceElementsWithFuncOfElemsAround(x => x.GetElements().Max(), windowSize, zeroPaddedEdges);

            return result;
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
