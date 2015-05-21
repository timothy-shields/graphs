
namespace Shields.Graphs
{
    /// <summary>
    /// Represents a weighted value.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    public interface IWeighted<T>
    {
        /// <summary>
        /// The value.
        /// </summary>
        T Value { get; }
        
        /// <summary>
        /// The weight of the value.
        /// </summary>
        double Weight { get; }
    }
}
