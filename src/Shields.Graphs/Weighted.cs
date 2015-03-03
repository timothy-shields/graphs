
namespace Shields.Graphs
{
    public class Weighted<T> : IWeighted<T>
    {
        public Weighted(T value, double weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        public T Value { get; private set; }

        public double Weight { get; private set; }
    }
}
