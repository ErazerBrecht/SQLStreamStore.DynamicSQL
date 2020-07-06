using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace StreamStore.DynamicSQL
{
    public interface IEventBomber
    {
        Task Bomb();
    }

    public class EventBomber : IEventBomber
    {
        private const string StreamName = "TestPostgresPerformanceStream";
        private const long MaxAmountOfEvents = 2500000;
        private readonly IStreamStore _streamStore;
        private static readonly Random Rand = new Random();

        public EventBomber(IStreamStore streamStore)
        {
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
        }

        public async Task Bomb()
        {
            var current = await _streamStore.ReadHeadPosition();
            var amountOfEvents = MaxAmountOfEvents - (current + 1);
            
            while (amountOfEvents > 0)
            {
                var newBatch = amountOfEvents > 2500 ? 2500: (int) amountOfEvents;
                var newEvents = Enumerable.Range(0, newBatch).Select((_, i) => 
                    new 
                    { 
                        Index = i, 
                        Message = new NewStreamMessage(Guid.NewGuid(), "RandomEvent", JsonSerializer.Serialize(new Event
                        {
                            Random = Rand.NextDouble()
                        }))
                    }).ToArray();

                var tasks = newEvents
                    .GroupBy(x => x.Index / 50)
                    .Select(x => x.Select(y => y.Message).ToArray())
                    .Select(x =>
                        _streamStore.AppendToStream($"{StreamName}/{Guid.NewGuid()}", ExpectedVersion.Any, x));

                await Task.WhenAll(tasks);
                amountOfEvents -= newBatch;
                Console.WriteLine($"Still need to add: {amountOfEvents} events");
            }
        }
    }
}