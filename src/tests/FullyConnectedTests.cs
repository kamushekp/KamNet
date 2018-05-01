using FeedForward;
using FeedForward.FeedForwardLayers;
using FeedForwardNet;
using NUnit.Framework;
using System.Numerics;

namespace Tests
{
    public class FullyConnectedTests
    {
        private Net net;

        [Test]
        public void SimpleFeedForwardWorks()
        {
            var fc1 = new FullyConnectedLayer(5, 3, x => x);
            fc1.FillBiases(new float[] { -1f, 1f, -2f });
            fc1.FillWeights(1.0f);

            var fc2 = new FullyConnectedLayer(3, 2, x => x);
            fc2.FillBiases(new float[] { -5, 5 });
            fc2.FillWeights(2.0f);

            var net = new Net(fc1, fc2);

            var input = new DenseTensor<float>(new int[] { 5 });
            input.FillWithFunction(indices => indices[0]);

            var classified = net.Classify(input);
            Assert.AreEqual(1, classified);
        }
    }
}
