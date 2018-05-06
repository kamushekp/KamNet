using FeedForward;
using FeedForward.FeedForwardLayers;
using FeedForwardNet;
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
            d1 = new DenseTensor<float>(new int[] { 2, 1 });
            d2 = new DenseTensor<float>(new int[] { 2, 1 });
            d3 = new DenseTensor<float>(new int[] { 2, 1 });
            d4 = new DenseTensor<float>(new int[] { 2, 1 });

            d1[0, 0] = 0f;
            d1[1, 0] = 0f;

            d2[0, 0] = 0f;
            d2[1, 0] = 1f;

            d3[0, 0] = 1f;
            d3[1, 0] = 0f;

            d4[0, 0] = 1f;
            d4[1, 0] = 1f;
        }

        [Test]
        public void SimpleFeedForwardWorks()
        {
            var fc1 = new FullyConnectedLayer(5, 3, x => x, x => 1);

            var fc2 = new FullyConnectedLayer(3, 2, x => x, x => 1);

            var net = new Net(CostFunction.quadratic, fc1, fc2);

            var input = new DenseTensor<float>(new int[] { 5 , 1 });
            input.FillWithFunction(indices => indices[0]);

            var classified = net.Classify(input);
            Assert.AreEqual(1, classified);
        }

        [Test]
        public void SumWorks()
        {
            var testData = new Tensor<float>[4] { d1, d2, d3, d4 }.
                SelectMany(x => new Tensor<float>[4] { x, x, x, x }).
                ToArray();

            var zero = new DenseTensor<float>(new int[] { 1, 1 }).FillWithValue(0f);
            var one = new DenseTensor<float>(new int[] { 1, 1 }).FillWithValue(1f);
            var two = new DenseTensor<float>(new int[] { 1, 1 }).FillWithValue(2f);

            var answers = new Tensor<float>[16] {
                zero, zero, zero, zero,
                one, one, one, one,
                one, one, one, one,
                two, two, two, two };

            var fc1 = new FullyConnectedLayer(2, 1, x => x, x => 1);
            var net = new Net(CostFunction.quadratic, fc1);

            net.Learn(testData, answers);

            var twoAndThree = new DenseTensor<float>(new int[] { 2, 1 });
            twoAndThree[0, 0] = 2f;
            twoAndThree[1, 0] = 3f;

            Assert.AreEqual(5f, net.GetOutput(twoAndThree)[0, 0], 0.00001);
        }
    }
}
