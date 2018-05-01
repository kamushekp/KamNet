using System;
using System.Collections.Generic;
using System.Text;

namespace FeedForward.Core
{
    public partial class Matrix
    {
        public void Add(Matrix matrix)
        {
            for (int i = allowedFromInclusiveY; i < allowedToExclusiveY; i++)
            {
                for (int j = allowedFromInclusiveX; j < allowedToExclusiveX; j++)
                {
                    data[i][j] += matrix.data[i][j];
                }
            }
        }

    }
}
