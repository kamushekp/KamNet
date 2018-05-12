using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FeedForwardNet.CostFunctions
{
    public class Quadratic : ICostFunction
    {
        public float Calculate(Tensor<float>[] expected, Tensor<float>[] actual)
        {
            var indexes = new int[expected.Length];
            return indexes.
                Select(index => CalculateForUnit(expected[index], actual[index])).
                Sum() / indexes.Length;
        }

        public float CalculateForUnit(Tensor<float> expected, Tensor<float> actual)
        {
            return Sub(expected, actual).Select(x => x * x).Sum() / 2f;
        }

        public Tensor<float> GradientForUnitByActivation(Tensor<float> expected, Tensor<float> actual)
        {
            var sub = Sub(actual, expected);
            return sub;
        }

        private Tensor<float> Sub(Tensor<float> x, Tensor<float> y) => x - y;
    }
}
