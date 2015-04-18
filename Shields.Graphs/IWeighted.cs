
namespace Shields.Graphs
{
    public interface IWeighted<T>
    {
        T Value { get; }
        double Weight { get; }
    }
}
