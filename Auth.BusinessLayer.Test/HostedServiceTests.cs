using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class HostedServiceTests
{

    #region SetUp

    #pragma warning disable CS8618
    private IMemoryCache _cache;
    private Mock<IInitializationConfigs> _configs;
    private Mock<IInitializationLeads> _leads;
    private Mock<IHostApplicationLifetime> _lifetime;
    private HostedService _hosted;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _configs = new Mock<IInitializationConfigs>();
        _leads = new Mock<IInitializationLeads>();
        _lifetime = new Mock<IHostApplicationLifetime>();

        _hosted = new HostedService(_lifetime.Object, _configs.Object, _leads.Object, _cache);
    }

    #endregion

    [Test]
    public async Task ExecuteAsync_ApplicationStarted_ShouldWork()
    {
        //given
        _lifetime.Setup(s => s.ApplicationStarted).Returns(new CancellationTokenSource(TimeSpan.Zero).Token);
        _cache.Set("Initialization leads", true);

        //when
        await _hosted.Test(CancellationToken.None);

        //then
        _configs.Verify(v => v.InitializeConfigs(), Times.Once);
        _leads.Verify(v => v.InitializeLeads(), Times.Once);
        Assert.AreEqual(_cache.Get<Dictionary<Microservice, MicroserviceModel>>(nameof(Microservice)), InitializeMicroserviceModels.InitializeMicroservices());
        Assert.AreEqual(_cache.Get<Task>("Initialization task configs").Status, TaskStatus.RanToCompletion);
        Assert.AreEqual(_cache.Get<Task>("Initialization task lead").Status, TaskStatus.RanToCompletion);
    }

    [Test]
    public async Task ExecuteAsync_ApplicationStopped_ShouldStopApplication()
    {
        //given
        _cache.Set("Initialization leads", true);

        //when
        await _hosted.Test(new CancellationTokenSource(TimeSpan.Zero).Token);

        //then
        _configs.Verify(v => v.InitializeConfigs(), Times.Never);
        _leads.Verify(v => v.InitializeLeads(), Times.Never);
        Assert.IsNull(_cache.Get<Dictionary<Microservice, MicroserviceModel>>(nameof(Microservice)));
        Assert.AreEqual(_cache.Get<Task>("Initialization task configs").Status, TaskStatus.Created);
        Assert.AreEqual(_cache.Get<Task>("Initialization task lead").Status, TaskStatus.Created);
    }
}