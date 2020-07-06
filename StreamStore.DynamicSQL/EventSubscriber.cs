using System;
using System.Threading;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace StreamStore.DynamicSQL
{
    public interface IEventSubscriber
    {
        Task Subscribe();
    }

    public class EventSubscriber : IEventSubscriber
    {
        private readonly IStreamStore _streamStore;

        public EventSubscriber(IStreamStore streamStore)
        {
            _streamStore = streamStore;
        }

        public Task Subscribe()
        {
            var tsc = new TaskCompletionSource<bool>();
            var subscription = _streamStore.SubscribeToAll(0, StreamMessageReceived, null, up =>
            {
                if (up) tsc.SetResult(true);
            });
            subscription.MaxCountPerRead = 500;
            
            return tsc.Task;
        }

        private static async Task StreamMessageReceived(IAllStreamSubscription subscription, StreamMessage streamMessage, CancellationToken cancellationToken)
        {
            var data = await streamMessage.GetJsonDataAs<Event>(cancellationToken);
            Console.WriteLine($"Message position: {streamMessage.Position} | Message id: {streamMessage.MessageId}");
        }
    }
}