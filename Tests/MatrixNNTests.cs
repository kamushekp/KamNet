using KamNet;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    public class MatrixNNTests
    {
        private double[][] data;

        /// <summary>
        /// | 0  1  2  3  4  |
        /// | 5  6  7  8  9  |
        /// | 10 11 12 13 14 |
        /// | 15 16 17 18 19 |
        /// | 20 21 22 23 24 |
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            data = new double[5].
                Select
                (
                (row, index) => Enumerable.Range(index * 5, 5).Select(x => (double)x).ToArray()
                ).ToArray();
        }

        [Test]
        public void CanConvolveTwoSameSizeMatrix()
        {
            var first = new Matrix(data, 0, 2, 0, 2, useOriginal: false);
            var second = new Matrix(data, 3, 5, 3, 5, useOriginal: false);
            var a = second.GetElements().ToArray();

            var convResult = first.Convolve(second);
            var expected = 0 * 18 + 1 * 19 + 5 * 23 + 6 * 24;
            Assert.AreEqual(expected, convResult);
        }

        [Test]
        public void CanConvolveBigMatrixWithKernel()
        {
            var big = new Matrix(data: data);
            var second = new Matrix(rows: 3, columns: 3, initializer: 1.0);

            var convResult = big.KernelPooling(second);
        }
    }
}
