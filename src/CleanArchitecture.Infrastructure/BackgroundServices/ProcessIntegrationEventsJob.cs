using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence.DbContexts;
using CleanArchitecture.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.BackgroundServices;

public class ProcessIntegrationEventsJob(
    IServiceScopeFactory scopeFactory,
    IIntegrationEventBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var messages = await context.Set<IntegrationOutboxMessage>()
                .Where(m => m.ProcessedAtUtc == null)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    var type = Type.GetType(message.Type);
                    var @event = JsonSerializer.Deserialize(message.Content, type!);

                    // Use reflection to call the generic SendAsync
                    await (Task)bus.GetType()
                        .GetMethod(nameof(IIntegrationEventBus.SendAsync))!
                        .MakeGenericMethod(type!)
                        .Invoke(bus, [@event, stoppingToken])!;

                    message.ProcessedAtUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.Error = ex.ToString();
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }
}