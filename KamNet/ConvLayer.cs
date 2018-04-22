using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KamNet
{
    public class ConvLayer
    {
        private int featureMapsInput;
        private int neurons;

        private ConvolutionalKernel[][] kernels;
        private double[] biases;
        private Func<double, double> a;

        private Matrix[][] deltaKernels;
        private double[] deltaBiases;

        private const int kernelWidth = 5;

        private int widthInp;

        public ConvLayer(int featureMapsInput, int neurons, Func<double, double> activationFunction, int widthInp)
        {
            kernels = new ConvolutionalKernel[featureMapsInput][].
                Select(x => new ConvolutionalKernel[neurons].Select(_ => new ConvolutionalKernel(kernelWidth)).ToArray()).
                ToArray();
            biases = new double[neurons].Select(x => 1.0).ToArray();
            a = activationFunction;

            this.featureMapsInput = featureMapsInput;
            this.neurons = neurons;
            this.widthInp = widthInp;
        }

        Matrix[] Forward(Matrix[] input)
        {
            var result = new Matrix[neurons];
            
            for (int i = 0; i < neurons; i++)
            {
                var sum = new Matrix(widthInp - kernelWidth + 1, widthInp - kernelWidth + 1, 0.0);  //TODO
                for (int j = 0; j < featureMapsInput; j++)
                {
                    var convolved = kernels[j][i].ApplyOnMatrix(input[j]);
                    sum.Add(convolved);
                }

                sum.ApplyElementwiseFunction(x => a(x + biases[i]));
                //result[i] = sum.AveragePooling(2);
            }

            return result;
        }

        void UpdateErrors(Matrix[] deltaOfNextLayer)
        {
            
        }
    }
}
