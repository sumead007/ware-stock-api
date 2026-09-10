using System.Runtime.CompilerServices;
using AutoMapper;
using WareStockApi.Application.Chats;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Integrations;
using WareStockApi.Application.Products;
using WareStockApi.Application.Stock;
using WareStockApi.Application.Tasks;
using WareStockApi.Domain.Entities;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace WareStockApi.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private ILoggerFactory? _loggerFactory;
    private MapperConfiguration? _configuration;
    private IMapper? _mapper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Minimal logger factory for tests
        _loggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

        _configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(IApplicationDbContext).Assembly),
            loggerFactory: _loggerFactory);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration!.AssertConfigurationIsValid();
    }

    [Test]
    [TestCase(typeof(Product), typeof(ProductDto))]
    [TestCase(typeof(WorkTask), typeof(TaskDto))]
    [TestCase(typeof(StockTransaction), typeof(StockTransactionDto))]
    [TestCase(typeof(Conversation), typeof(ConversationDto))]
    [TestCase(typeof(Message), typeof(MessageDto))]
    [TestCase(typeof(Integration), typeof(IntegrationDto))]
    [TestCase(typeof(ProductCategory), typeof(LookupDto))]
    [TestCase(typeof(ProductUnit), typeof(LookupDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        _mapper!.Map(instance, source, destination);
    }

    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }


    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _loggerFactory?.Dispose();
    }
}
