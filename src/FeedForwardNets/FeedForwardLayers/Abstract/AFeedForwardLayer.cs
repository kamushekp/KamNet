using FeedForward.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FeedForward.FeedForwardLayers.Interfaces
{
    public abstract class AFeedForwardLayer
    {
        public Func<float, float> A { get; }
        public Func<float, float> DA { get; }

        public int InputCount { get; }
        public int OutputCount { get; }

        public Tensor<float> Weights { get; protected set; }
        public Tensor<float> Biases { get; protected set; }

        public Tensor<float> LastInput { get; set; }
        public Tensor<float> LastOutput { get; set; }
        public Tensor<float> LastError { get; set; }

        public abstract void UpdateParametres();
        public abstract Tensor<float> FeedForward(Tensor<float> input);
        public abstract void AccumulateSampleError(AFeedForwardLayer nextLayer);
        public abstract void ProcessCostFunctionGradient(Tensor<float> outputError);

        public AFeedForwardLayer(
            int inputCount,
            int neuronsCount, 
            Func<float, float> activationFunction,
            Func<float, float> derivativeOfActivationFunction)
        {
            InputCount = inputCount;
            OutputCount = neuronsCount;
            A = activationFunction;
            DA = derivativeOfActivationFunction;
        }
    }
}
