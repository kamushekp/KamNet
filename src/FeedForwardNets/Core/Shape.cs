using System;
using System.Collections.Generic;
using System.Text;

namespace FeedForward
{
    public class Shape
    {
        public uint[] DimensionsSizes { get; }

        public Shape(uint[] shape)
        {
            DimensionsSizes = new uint[shape.Length];
            shape.CopyTo(DimensionsSizes, 0);
        }
    }

}
