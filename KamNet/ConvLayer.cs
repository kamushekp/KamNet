using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KamNet
{
    public class ConvolutionalPoolingLayer
    {
        private Matrix[] inputFeatureMaps;
        private int neuronsCount;

        private Matrix[][] sharedKernels;
        private double[] biases;
        private Func<double, double> activationFunction;

        private Matrix[][] deltaKernels;
        private double[] deltaBiases;

        private const int kernelWidth = 5;

        private int widthInp;

        public ConvolutionalPoolingLayer(int featureMapsInput, int neurons, Func<double, double> activationFunction, int widthInp)
        {

        }

        Matrix[] FeedForward(Matrix[] input)
        {
            inputFeatureMaps = input;

            var result = new Matrix[neuronsCount];
            for (int i = 0; i < neuronsCount; i++)
            {
                Matrix sum = null;
                for (int j = 0; j < inputFeatureMaps.Length; j++)
                {
                    var convolved = inputFeatureMaps[j].KernelPooling(sharedKernels[j][i]);
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
                
                result[i] = sum.MaxPooling(2);
            }

            return result;
        }

        void Backpropagation(Matrix[] deltaOfNextLayer)
        {
            
        }
    }
}
