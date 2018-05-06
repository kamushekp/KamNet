using FeedForward.Core;
using FeedForward.FeedForwardLayers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeedForward.FeedForwardLayers
{
    internal class ConvolutionalPoolingLayer// : IFeedForwardLayer
    {
        public int InputCount { get; set; }
        public Shape InputShape { get; set; }
        public int KernelsSize { get; set; }
        public int NeuronsCount { get; set; }

        private Matrix[] lastInput;
        private int inputCount;

        private Matrix[][] sharedKernels;
        private double[] biases;

        private Matrix[][] deltaKernels;
        private double[] deltaBiases;

        private Func<double, double> activationFunction;

        private Matrix[] lastOutput;
        private int outputCount;

        public ConvolutionalPoolingLayer()
        {
        }

        public Matrix[] FeedForward(Matrix[] input)
        {
            lastInput = input;

            var result = new Matrix[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                Matrix sum = null;
                for (int j = 0; j < lastInput.Length; j++)
                {
                    var convolved = lastInput[j].KernelConvolution(sharedKernels[j][i]);
                    if (sum == null)
                    {
                        sum = convolved;
                    }
                    else
                    {
                        sum.Add(convolved);
                    }
                }
                sum.ApplyElementwiseFunction(x => activationFunction(x + biases[i]));
                
                result[i] = sum.Average2x2Pooling();
            }

            return result;
        }

        public void Backpropagation(Matrix[] errorOfNextLayer)
        {
            var deltaC = new Matrix[outputCount];

            for (int i = 0; i < outputCount; i++)
            {
                deltaC[i] = UpsampleFromAvgPooling(errorOfNextLayer[i]);
            }

            double deltaC_q_sigma_rule(int q, int i, int j) => deltaC[q][i, j] * lastOutput[q][i, j] * (1 - lastOutput[q][i, j]);

            for (int q = 0; q < outputCount; q++)
            {
                var deltaC_q_sigma = new Matrix(deltaC[q].Height, deltaC[q].Width);
                deltaC_q_sigma.ApplyElementwiseFunction((i, j) => deltaC_q_sigma_rule(q, i, j));

                for (int p = 0; p < inputCount; p++)
                {
                    deltaKernels[p][q] = lastInput[p].Get180Rotated().KernelConvolution(deltaC_q_sigma);
                }
            }
        }

        private Matrix UpsampleFromAvgPooling(Matrix pooled)
        {
            var n = pooled.Height * 2;
            var m = pooled.Width * 2;

            var result = new Matrix(n, m);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var y = (int)Math.Ceiling((i + 1) / 2.0);
                    var x = (int)Math.Ceiling((j + 1) / 2.0);

                    result[i, j] = 0.25 * pooled[y, x];
                }
            }

            return result;
        }

        public Matrix[] GetError(Matrix[] expected)
        {
            throw new NotImplementedException();
        }
    }
}
