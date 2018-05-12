using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FeedForwardNet
{
    public interface ICostFunction
    {
        float Calculate(Tensor<float>[] expected, Tensor<float>[] actual);
        float CalculateForUnit(Tensor<float> expected, Tensor<float> actual);

        Tensor<float> GradientForUnitByActivation(Tensor<float> expected, Tensor<float> actual);
    }
}
