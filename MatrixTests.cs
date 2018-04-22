using KamNet;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class MatrixTests
    {
        private double[][] data;
        private Matrix bigMatrix;
        private Matrix smallMatrix;

        [Test, Order(0)]
        public void CanCreateFromJaggedArray()
        {
            data = new double[3][].
                Select(
                (row, i) => new double[4].Select((element, j) => (double)CantorsNumber(i, j)).ToArray()
                ).ToArray();
            bigMatrix = new Matrix(data);

            var inMatrix = bigMatrix.GetElements().ToArray();
            var original = data.SelectMany(row => row.ToArray()).ToArray();

            CollectionAssert.AreEqual(original, inMatrix);
        }

        [Test, Order(0)]
        public void ChangesInOriginalDataAffectMatrix()
        {
            data[1][1] = 55;

            Assert.AreEqual(data[1][1], bigMatrix[1, 1]);
        }

        private int CantorsNumber(int i, int j) => (i + j + 1) * (i + j) / 2 + i;
    }
}
