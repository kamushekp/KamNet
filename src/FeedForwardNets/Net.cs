using FeedForward.Core;
using FeedForward.FeedForwardLayers;
using FeedForward.FeedForwardLayers.Interfaces;
using FeedForwardNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FeedForward
{
    public class Net
    {
        private List<IFeedForwardLayer> _layers = new List<IFeedForwardLayer>();

        public Net(params IFeedForwardLayer[] layers)
        {
            foreach (var layer in layers)
            {
                _layers.Add(layer);
            }
        }

        public int Classify(Tensor<float> input)
        {
            var result = input;
            foreach (var layer in _layers)
            {
                result = layer.FeedForward(result);
            }

            var maxIndex = result.GetIndexOfMax();
            return maxIndex;
        }

        public void Learn(Tensor<float> input)
        {

        }

    }
}
