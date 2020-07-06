using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SqlStreamStore;

namespace StreamStore.DynamicSQL
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEventStore(this IServiceCollection services, string connectionString)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(nameof(connectionString));

            var settings = new PostgresStreamStoreSettings(connectionString);

            services.AddScoped<IDbConnection>(ctx => new NpgsqlConnection(connectionString));
            services.AddSingleton<PostgresStreamStore>(ctx => new PostgresStreamStore(settings));
            services.AddSingleton<IStreamStore>(ctx => ctx.GetService<PostgresStreamStore>());
        }
    }
}