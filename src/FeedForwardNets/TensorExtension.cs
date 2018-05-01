using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FeedForwardNet
{
    public static class TensorExtension
    {
        public static uint[] GetIndices<T>(this Tensor<T> tensor, uint index)
        {
            var remainder = index;
            var strides = tensor.Strides;
            var result = new uint[strides.Length];

            for (int i = 0; i < strides.Length; i++)
            {
                var stride = (uint)strides[i];
                result[i] = remainder / stride;
                remainder %= stride;
            }

            return result;
        }

        public static void FillWithFunction<T>(this Tensor<T> tensor, Func<uint[], T> fillRule)
        {
            var elementsCount = tensor.Length;

            for (int i = 0; i < elementsCount; i++)
            {
                var indices = tensor.GetIndices((uint)i);
                var filler = fillRule(indices);
                tensor.SetValue(i, filler);
            }
        }

        public static int GetIndexOfMax(this Tensor<float> tensor)
        {
            if (tensor.Length <= 0)
            {
                throw new ArgumentException();
            }

            var maxIndex = 0;
            var currentIndex = 0;
            var max = tensor.GetValue(maxIndex);

            foreach (var x in tensor)
            {
                var currentValue = tensor.GetValue(currentIndex);
                if (currentValue >= max)
                {
                    max = currentValue;
                    maxIndex = currentIndex;
                }
                currentIndex++;
            }

            return maxIndex;
        }
    }
}
