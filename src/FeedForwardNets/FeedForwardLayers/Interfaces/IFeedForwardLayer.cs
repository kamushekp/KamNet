using FeedForward.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FeedForward.FeedForwardLayers.Interfaces
{
    public interface IFeedForwardLayer
    {
        int NeuronsCount { get; }

        Tensor<float> FeedForward(Tensor<float> input);
        Tensor<float> GetError(Tensor<float> expected);
        void Backpropagation(Tensor<float> deltaS);
    }
}
