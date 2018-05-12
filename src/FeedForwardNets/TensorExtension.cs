using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FeedForwardNet
{
    public static class TensorExtension
    {
        /// <summary>
        ///     Получает из индекса в одномерном куске памяти (где на самом деле хранятся данные)
        ///     индекс в многомерном пространстве тензора.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <param name="index"></param>
        /// <returns></returns>
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


        public static void FillWithValues(this Tensor<float> tensor, params float[] values)
        {
            if (tensor.Length != values.Length)
            {
                throw new ArgumentException();
            }
            for(int i = 0; i < tensor.Length; i++)
            {
                tensor.SetValue(i, values[i]);
            }
        }

        /// <summary>
        ///     Заполняет тензор объектами типа T согласно функции от набора индексов.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <param name="fillRule"></param>
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
        
        /// <summary>
        ///     Заполняет тензор, используя функцию float -> float
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="fillRule"></param>
        public static void FillWithFunction(this Tensor<float> tensor, Func<float, float> fillRule)
        {
            var elementsCount = tensor.Length;

            for (int i = 0; i < elementsCount; i++)
            {
                var value = tensor.GetValue(i);
                tensor.SetValue(i, fillRule(value));
            }
        }

        /// <summary>
        ///     Транспонирование для матриц (тензоров с Dimensions.Length = 2)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static Tensor<T> Transpose<T>(this Tensor<T> tensor)
        {
            if (tensor.Dimensions.Length != 2)
            {
                throw new ArgumentException("Данное действие определено только для матриц.");
            }

            var rows = tensor.Dimensions[0];
            var cols = tensor.Dimensions[1];

            var result = new DenseTensor<T>(new[] { cols, rows });

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[j, i] = tensor[i, j];
                }
            }

            return result;
        }

        /// <summary>
        ///     Ищет индекс максимального числа в одномерном куске памяти.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
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
