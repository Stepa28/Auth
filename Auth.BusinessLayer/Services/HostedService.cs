﻿using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

namespace Auth.BusinessLayer.Services;

public class HostedService : BackgroundService
{
    private readonly IInitializationConfigs _configs;
    private readonly IInitializationLeads _leads;
    private readonly IInitializeMicroserviceModels _microservice;
    private readonly IMemoryCache _cache;
    private readonly IHostApplicationLifetime _lifetime;

    public HostedService(IHostApplicationLifetime lifetime, IInitializationConfigs configs, IInitializationLeads leads, IMemoryCache cache,
        IInitializeMicroserviceModels microservice)
    {
        _lifetime = lifetime;
        _configs = configs;
        _leads = leads;
        _cache = cache;
        _microservice = microservice;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppStartup(_lifetime, stoppingToken))
            return;
        // Приложение запущено и готово к обработке запросов

        //запуск инициализации конфигурации
        await _configs.InitializeConfigs();
        _cache.Set(nameof(Microservice), _microservice.InitializeMicroservices());

        //запуск инициализации кеша лидов(если не удалось повторить через час)
        do
        {
            await _leads.InitializeLeadsAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        } while (!_cache.Get<bool>("Initialization"));
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
        Task completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        // Если завершилась задача ApplicationStarted, возвращаем true, иначе false
        return completedTask == startedSource.Task;
    }
}