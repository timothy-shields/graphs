using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    public interface IWeighted<T>
    {
        T Value { get; }
        double Weight { get; }
    }
}
