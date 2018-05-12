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
    public class Net
    {
        private int layersCount;
        private List<AFeedForwardLayer> layers;

        private ICostFunction costFunction;

        public Net(ICostFunction costFunction, params AFeedForwardLayer[] layers)
        {
            this.layers = new List<AFeedForwardLayer>(layers);
            this.layersCount = layers.Length;
            this.costFunction = costFunction;
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

        public void Learn(Tensor<float>[] input, Tensor<float>[] expectedOutput)
        {
            var rand = new Random(1234);
            var batchSize = 20;

            for (int i = 0; i < 10000; i++)
            {
                var indexes = new int[batchSize].
                    Select(x => rand.Next(input.Length)).
                    ToArray();

                var batch = indexes.Select(index => input[index]).ToArray();
                var actual = indexes.Select(index => expectedOutput[index]).ToArray();

                var costOnBatch = LearnOnBatch(batch, actual);
                Console.WriteLine(costOnBatch);
            }
        }

        private float LearnOnBatch(Tensor<float>[] batch, Tensor<float>[] expected)
        {
            var outputs = new List<Tensor<float>>();

            for (int i = 0; i < batch.Length; i++)
            {
                var oneFromBatch = batch[i];
                var feedForwarded = GetOutput(oneFromBatch);
                var costGrad = costFunction.GradientForUnitByActivation(expected[i], feedForwarded);

                AccumulateUnitError(costGrad);

                outputs.Add(feedForwarded);
            }

            foreach (var layer in layers)
            {
                layer.UpdateParameters();
            }

            return 0f;
        }

        private void AccumulateUnitError(Tensor<float> error)
        {
            layers[layersCount - 1].ProcessCostFunctionGradient(error);

            for (int i = layersCount - 2; i >= 0; i--)
            {
                layers[i].AccumulateSampleError(layers[i + 1]);
            }
        }

    }
}
