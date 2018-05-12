using FeedForward;
using FeedForward.FeedForwardLayers;
using FeedForwardNet;
using FeedForwardNet.CostFunctions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Numerics;

namespace Tests
{
    public class FullyConnectedTests
    {
        Tensor<float> d1, d2, d3, d4;

        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void SumWorks()
        {
            d1 = new DenseTensor<float>(new int[] { 2, 1 });
            d2 = new DenseTensor<float>(new int[] { 2, 1 });
            d3 = new DenseTensor<float>(new int[] { 2, 1 });
            d4 = new DenseTensor<float>(new int[] { 2, 1 });

            d1.FillWithValues(0f, 0f);
            d2.FillWithValues(0f, 1f);
            d3.FillWithValues(1f, 0f);
            d4.FillWithValues(1f, 1f);

            var testData = new Tensor<float>[4] { d1, d2, d3, d4 }.
                SelectMany(x => new Tensor<float>[4] { x, x, x, x }).
                ToArray();

            var zero = new DenseTensor<float>(new int[] { 1, 1 });
            var one = new DenseTensor<float>(new int[] { 1, 1 });
            var two = new DenseTensor<float>(new int[] { 1, 1 });

            one.Fill(1f);
            two.Fill(2f);

            var answers = new Tensor<float>[16] {
                zero, zero, zero, zero,
                one, one, one, one,
                one, one, one, one,
                two, two, two, two };

            var fc1 = new FullyConnectedLayer(2, 1, x => x, x => 1);
            var net = new Net(new Quadratic(), fc1);

            net.Learn(testData, answers);

            var twoAndThree = new DenseTensor<float>(new int[] { 2, 1 });
            twoAndThree[0, 0] = 2f;
            twoAndThree[1, 0] = 3f;

            Assert.AreEqual(5f, net.GetOutput(twoAndThree)[0, 0], 0.00001);
        }

        /// <summary>
        ///     На входе A,B вычисляет A^2 - B^2
        /// </summary>
        [Test]
        public void FuncWorks()
        {
            var foo = 10;
            var rand = new Random(123);

            var testData = new Tensor<float>[foo * foo * foo].
                Select(x => new DenseTensor<float>(new int[] { 2, 1 })).
                Select(x =>
                {
                    var a = rand.NextDouble();
                    var b = rand.NextDouble();
                    x.FillWithValues((float)a, (float)b);
                    return x;
                }).
                ToArray();

            var answers = new Tensor<float>[foo * foo * foo].
                Select(x => new DenseTensor<float>(new int[] { 1, 1 })).
                Select((x, i) =>
                {
                    var a = testData[i][0, 0];
                    var b = testData[i][0, 1];
                    x.FillWithValues(a * a - b * b);
                    return x;
                }).
                ToArray();

            var fc1 = new FullyConnectedLayer(2, 4, x => x, x => 1);
            var fc2 = new FullyConnectedLayer(4, 1, x => x, x => 1);

            var net = new Net(new Quadratic(), fc1, fc2);

            net.Learn(testData, answers);


            var first = 0.5f;
            var second = 0.57f;

            var input = new DenseTensor<float>(new int[] { 2, 1 });
            input.FillWithValues(first, second);

            var output = net.GetOutput(input)[0, 0];
            var expected = first * first - second * second;
        }
    }
}
