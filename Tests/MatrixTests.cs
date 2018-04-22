using KamNet;
using NUnit.Framework;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        private double[][] data;
        
        /// <summary>
        ///     Инициализирует следующую матрицу:
        /// |  0  2  5  9  |
        /// |  1  4  8  13 |
        /// |  3  7  12 18 |
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            data = new double[3][].
                Select(
                (row, i) => new double[4].Select((element, j) => (double)CantorsNumber(i, j)).ToArray()
                ).ToArray();
        }

        [Test]
        public void CanCreateFromJagged()
        {
            var bigMatrix = new Matrix(data);

            var inMatrix = bigMatrix.GetElements().ToArray();
            var original = data.SelectMany(row => row.ToArray()).ToArray();

            data[1][1] = 55;

            CollectionAssert.AreEqual(original, inMatrix);
        }

        [Test]
        public void CanSliceFromJagged()
        {
            var submatrix = new Matrix(data, 2, 4, 1, 3);
            var should = new[] { 8, 13, 12, 18 };
            var actual = submatrix.GetElements().ToArray();
            CollectionAssert.AreEquivalent(should, actual);
        }

        [Test]
        public void ChangesInOriginalDataAffectMatrix()
        {
            var bigMatrix = new Matrix(data);
            data[1][1] = data[1][1] * 2;

            Assert.AreEqual(data[1][1], bigMatrix[1, 1]);
        }

        [Test]
        public void ChangesInSliceSourseAffectMatrix()
        {
            var submatrix = new Matrix(data, 2, 4, 1, 3);
            data[2][3] *= 2;
            Assert.AreEqual(data[2][3], submatrix[1, 1]);
        }

        private int CantorsNumber(int i, int j) => (i + j + 1) * (i + j) / 2 + j;

    }
}