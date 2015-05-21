
namespace Shields.Graphs
{
    /// <summary>
    /// Represents a weighted value.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    public class Weighted<T> : IWeighted<T>
    {
        /// <summary>
        /// Constructs a weighted value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="weight">The weight of the value.</param>
        public Weighted(T value, double weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        /// <summary>
        /// The value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// The weight of the value.
        /// </summary>
        public double Weight { get; private set; }
    }
}
