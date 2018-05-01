using FeedForward.Core;
using FeedForward.FeedForwardLayers.Interfaces;
using FeedForwardNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FeedForward.FeedForwardLayers
{
    public class FullyConnectedLayer : IFeedForwardLayer
    {
        private Tensor<float> biases;
        private Tensor<float> weights;

        public int InputCount { get; }
        public int NeuronsCount { get; }

        private Tensor<float> lastInput;
        private Tensor<float> lastOutput;

        private Func<float, float> activationFunction;

        public FullyConnectedLayer(int inputCount, int neuronsCount, Func<float, float> activationFunction)
        {
            InputCount = inputCount;
            NeuronsCount = neuronsCount;
            this.activationFunction = activationFunction;

            lastOutput = new DenseTensor<float>(neuronsCount);
            biases = new DenseTensor<float>(neuronsCount);
            weights = new DenseTensor<float>(new int[] { neuronsCount, inputCount });
        }

        public void SetBiases(Func<uint, float> FillingRule)
        {
            biases.FillWithFunction(indices => FillingRule(indices[0]));
        }

        public void SetWeights(Func<uint, uint, float> FillingRule)
        {
            weights.FillWithFunction(indices => FillingRule(indices[0], indices[1]));
        }

        public void FillBiases(float value)
        {
            this.SetBiases(index => value);
        }

        public void FillBiases(float[] values)
        {
            var oldDims = this.biases.Dimensions;
            this.biases = new DenseTensor<float>(new Memory<float>(values), oldDims);
        }

        public void FillWeights(float value)
        {
            this.SetWeights((neuron, inp) => value);
        }

        public void Backpropagation(Tensor<float> errorOfNextLayer)
        {
            throw new NotImplementedException();
        }

        public Tensor<float> FeedForward(Tensor<float> input)
        {
            lastInput = input;

            for (int neuron = 0; neuron < NeuronsCount; neuron++)
            {
                var sum = 0.0f;
                for (int inputElem = 0; inputElem < input.Length; inputElem++)
                {
                    sum += input[inputElem] * weights[neuron, inputElem];
                }

                sum += biases[neuron];

                lastOutput[neuron] = activationFunction(sum);
            }

            return lastOutput;
        }

        public Tensor<float> GetError(Tensor<float> expected)
        {
            if (lastOutput.Length != expected.Length)
            {
                throw new ArgumentException();
            }

            return expected - lastInput;
        }

        public override string ToString()
        {
            return $"Fully connected layer: input = {InputCount}, neurons = {NeuronsCount}";
        }
    }
}
