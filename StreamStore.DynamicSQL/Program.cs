using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;

namespace StreamStore.DynamicSQL
{
    static class Program
    {
        private static IServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            RegisterServices();
            
            var store = _serviceProvider.GetService<PostgresStreamStore>();
            await store.CreateSchemaIfNotExists();

            var bomber = _serviceProvider.GetService<IEventBomber>();
            await bomber.Bomb();

            var subscriber = _serviceProvider.GetService<IEventSubscriber>();
            await subscriber.Subscribe();

            DisposeServices();
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            
            collection.AddEventStore("Host=localhost;Port=5432;User Id=postgres;Password=admin;Database=streamstore;Maximum Pool Size=75");
            collection.AddScoped<IEventBomber, EventBomber>();
            collection.AddScoped<IEventSubscriber, EventSubscriber>();
            
            _serviceProvider = collection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            switch (_serviceProvider)
            {
                case null:
                    return;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}