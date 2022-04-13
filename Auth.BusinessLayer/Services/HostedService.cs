using Auth.BusinessLayer.Helpers;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

namespace Auth.BusinessLayer.Services;

public class HostedService : BackgroundService
{
    private readonly IMemoryCache _cache;
    private readonly IInitializationConfigs _configs;
    private readonly IInitializationLeads _leads;
    private readonly IHostApplicationLifetime _lifetime;

    public HostedService(IHostApplicationLifetime lifetime, IInitializationConfigs configs, IInitializationLeads leads, IMemoryCache cache)
    {
        _lifetime = lifetime;
        _configs = configs;
        _leads = leads;
        _cache = cache;
    }

    public async Task Test(CancellationToken stoppingToken)
    {
        await ExecuteAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var taskConfig = new Task(() => _configs.InitializeConfigs());
        _cache.Set("Initialization task configs", taskConfig);
        var taskLead = new Task(() => _leads.InitializeLeads().Wait(stoppingToken));
        _cache.Set("Initialization task lead", taskLead);

        if (!await WaitForAppStartup(_lifetime, stoppingToken))
            return;
        // Приложение запущено и готово к обработке запросов


        //запуск инициализации моделей микросервисов
        _cache.Set(nameof(Microservice), InitializeMicroserviceModels.InitializeMicroservices());

        //запуск инициализации конфигурации
        taskConfig.Start();
        await taskConfig.WaitAsync(stoppingToken);

        //запуск инициализации кеша лидов(если не удалось повторить через час)
        do
        {
            taskLead.Start();
            await taskLead.WaitAsync(stoppingToken);
            if (_cache.Get<bool>("Initialization leads"))
                return;
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        } while (true);
    }

    private static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        // 👇 Создаём TaskCompletionSource для ApplicationStarted
        var startedSource = new TaskCompletionSource();
        using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());

        // 👇 Создаём TaskCompletionSource для stoppingToken
        var cancelledSource = new TaskCompletionSource();
        using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

        // Ожидаем любое из событий запуска или запроса на остановку
        var completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        // Если завершилась задача ApplicationStarted, возвращаем true, иначе false
        return completedTask == startedSource.Task;
    }
}