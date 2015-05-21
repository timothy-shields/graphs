using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    /// <summary>
    /// The exception that is thrown when an unexpected cycle is found in a graph.
    /// </summary>
    [Serializable]
    public class GraphCycleException : Exception
    {
        public GraphCycleException() { }
        public GraphCycleException(string message) : base(message) { }
        public GraphCycleException(string message, Exception inner) : base(message, inner) { }
        protected GraphCycleException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
