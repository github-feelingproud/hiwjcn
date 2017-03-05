
namespace Lib.events
{
    public interface IConsumer<T>
    {
        void HandleEvent(T eventMessage);
    }
}
