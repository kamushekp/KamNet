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
    public class FullyConnectedLayer : AFeedForwardLayer
    {
        private Func<Tensor<float>, Tensor<float>> activation;
        private Func<Tensor<float>, Tensor<float>> activationDerivative;

        private float learningRate = 0.05f;

        #region накопительные переменные для обучения сети батчами
        private int batchesCount;
        private Tensor<float> biasesUpdates;
        private Tensor<float> weightsUpdates;
        #endregion

        public FullyConnectedLayer(
            int inputCount,
            int neuronsCount,
            Func<float, float> activationFunction,
            Func<float, float> derivativeOfActivationFunction): base(inputCount, neuronsCount, activationFunction, derivativeOfActivationFunction)
        {
            biasesUpdates = new DenseTensor<float>(new int[] { neuronsCount, 1 });
            weightsUpdates = new DenseTensor<float>(new int[] { neuronsCount, inputCount });

            activation = new Func<Tensor<float>, Tensor<float>>(inp =>
            { inp.FillWithFunction(activationFunction); return inp; });

            activationDerivative = new Func<Tensor< float >, Tensor<float> > (inp =>
            { inp.FillWithFunction(derivativeOfActivationFunction); return inp; });

            InitializeParametres();
        }

        private void InitializeParametres()
        {
            Biases = new DenseTensor<float>(new[] { OutputCount, 1 });
            Weights = new DenseTensor<float>(new int[] { OutputCount, InputCount });

            Biases.FillWithFunction(x => x * 0 + 1f / OutputCount);
            Weights.FillWithFunction(x => x * 0 + 1f / InputCount);
        }

        public override void UpdateParametres()
        {
            Biases -= biasesUpdates * (learningRate / batchesCount);
            Weights -= weightsUpdates * (learningRate / batchesCount);

            batchesCount = 0;
            biasesUpdates.Fill(0f);
            weightsUpdates.Fill(0f);
        }

        /// <summary>
        ///     Накопление изменений для весов и смещений слоя. (Копятся в рамках одного обучающего батча).
        /// </summary>
        public override void AccumulateSampleError(AFeedForwardLayer nextLayer)
        {
            if (nextLayer is FullyConnectedLayer FCLayer)
            {
                var weightsOfNextLayer = FCLayer.Weights;
                var errorOfNextLayer = FCLayer.LastError;

                var backpropagateBeforeActivation = weightsOfNextLayer.Transpose().MatrixMultiply(errorOfNextLayer);
                LastError = activationDerivative(LastOutput) * backpropagateBeforeActivation;

                UpdateParameters();
            }
        }

        public override void ProcessCostFunctionGradient(Tensor<float> costGrad)
        {
            LastError = activationDerivative(LastOutput) * costGrad;
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            biasesUpdates += LastError;
            weightsUpdates += LastError.MatrixMultiply(LastInput.Transpose());
            batchesCount++;
        }

        /// <summary>
        ///     Вычисление прямого прохода через слой
        /// </summary>
        /// <param name="input">    Вектор - столбец входных данных [inputCount x 1] </param>
        /// <returns>    Вектор - столбец выходных данных [neuronsCount x 1] </returns>
        public override Tensor<float> FeedForward(Tensor<float> input)
        {
            LastInput = input;

            var weightedInput = Weights.MatrixMultiply(input);

            LastOutput = activation(weightedInput + Biases);

            return LastOutput;
        }

        public override string ToString()
        {
            return $"Fully connected layer: input = {InputCount}, output = {OutputCount}";
        }
    }
}
