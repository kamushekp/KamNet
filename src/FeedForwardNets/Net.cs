using FeedForward.Core;
using FeedForward.FeedForwardLayers;
using FeedForward.FeedForwardLayers.Interfaces;
using FeedForwardNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FeedForward
{
    public enum CostFunction
    {
        quadratic
    }

    public class Net
    {
        private int layersCount;
        private List<AFeedForwardLayer> layers;
        private Func<Tensor<float>, Tensor<float>, Tensor<float>> costGradientByActivation;

        public Net(CostFunction costFunction = CostFunction.quadratic, params AFeedForwardLayer[] layers)
        {
            this.layers = new List<AFeedForwardLayer>(layers);
            this.layersCount = layers.Length;

            switch(costFunction)
            {
                case CostFunction.quadratic:
                    this.costGradientByActivation = (expected, actual) => actual - expected;
                    break;
            }
        }

        public Tensor<float> GetOutput(Tensor<float> input)
        {
            var result = input;
            foreach (var layer in layers)
            {
                result = layer.FeedForward(result);
            }
            return result;
        }

        public int Classify(Tensor<float> input)
        {
            var result = GetOutput(input);
            var maxIndex = result.GetIndexOfMax();
            return maxIndex;
        }

        public void GetNetError(Tensor<float>[] input, Tensor<float>[] actualOutput)
        {

        }

        public void Learn(Tensor<float>[] input, Tensor<float>[] actualOutput)
        {
            var rand = new Random(1234);
            var batchSize = 10;

            for (int i = 0; i < 10000; i++)
            {
                var indexes = new int[batchSize].
                    Select(x => rand.Next(input.Length)).
                    ToArray();

                var batch = indexes.Select(index => input[index]).ToArray();
                var actual = indexes.Select(index => actualOutput[index]).ToArray();
                LearnOnBatch(batch, actual);
            }
        }

        private void LearnOnBatch(Tensor<float>[] batch, Tensor<float>[] actual)
        {
            for (int i = 0; i < batch.Length; i++)
            {
                var oneFromBatch = batch[i];
                var feedForwarded = GetOutput(oneFromBatch);
                var expected = actual[i];
                var costGrad = costGradientByActivation(expected, feedForwarded);

                AccumulateSampleError(costGrad);
            }

            foreach (var layer in layers)
            {
                layer.UpdateParametres();
            }
        }
        private void AccumulateSampleError(Tensor<float> error)
        {
            layers[layersCount - 1].ProcessCostFunctionGradient(error);

            for (int i = layersCount - 2; i >= 0; i--)
            {
                layers[i].AccumulateSampleError(layers[i + 1]);
            }

        }

    }
}
