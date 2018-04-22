using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KamNet
{
    public class ConvolutionalKernel: Matrix
    {
        private ParallelOptions parallelOptions;

        private int width;
        private int half;

        public ConvolutionalKernel(int width, ParallelOptions parallelOptions = null) : base(width, width, 1.0)
        {
            this.parallelOptions = parallelOptions ?? new ParallelOptions();

            this.width = width;
            this.half = width / 2;
        }

        public Matrix ApplyOnMatrix(Matrix matrix, bool zeroPaddedEdges = false)
        {
            if (zeroPaddedEdges || width % 2 != 1)
            {
                throw new NotImplementedException();
            }
            else
            {
                var result = new Matrix(matrix.Height - width + 1, matrix.Width - width + 1);

                Parallel.For(half, matrix.Height - half, parallelOptions, y =>
                {
                    for (int x = half; x < matrix.Width - half; x++)
                    {
                        result[y - half, x - half] = Convolve(y, x, matrix);
                    }
                });

                return result;
            }
        }


        private double Convolve(int y, int x, Matrix matrix)
        {
            var sum = 0.0;

            for (int i = -half; i <= half; i++)
            {
                for (int j = -half; j <= half; j++)
                {
                    sum += matrix[i, j] * this[i + width, j + width];
                }
            }

            return sum;
        }
    }
}
