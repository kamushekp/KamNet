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
        public void Can_Pool_Big_Matrix_With_Kernel_And_No_Zero_PaddedEdges()
        {
            var big = new Matrix(data: data);
            var second = new Matrix(rows: 3, columns: 3, initializer: 1.0);

            var convResult = big.KernelPooling(second);

            CollectionAssert.AreEqual(new[] { 54, 63, 72, 99, 108, 117, 144, 153, 162 }, convResult.GetElements().ToArray());
        }

        [Test]
        public void Can_Pool_Big_Matrix_With_Kernel_And_Zero_PaddedEdges()
        {
            var big = new Matrix(data: data);
            var second = new Matrix(rows: 3, columns: 3, initializer: 1.0);

            var convResult = big.KernelPooling(second, zeroPaddedEdges: true);

            CollectionAssert.AreEqual(new[] { 12, 21, 27, 33, 24, 33, 54, 63, 72, 51, 63, 99, 108, 117, 81, 93, 144, 153, 162, 111, 72, 111, 117, 123, 84 }, convResult.GetElements().ToArray());
        }

        [Test]
        public void Can_Max_Pool_Big_Matrix_with_no_Zero_PaddedEdges()
        {
            var big = new Matrix(data: data);
            var maxPooled = big.MaxPooling(3, zeroPaddedEdges: false);
            CollectionAssert.AreEqual(new[] { 12, 13, 14, 17, 18, 19, 22, 23, 24 }, maxPooled.GetElements().ToArray());
        }
    }
}
