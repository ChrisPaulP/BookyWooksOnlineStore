using BookyWooks.Messaging.MassTransit;
using MassTransit;

namespace BookyWooks.Catalogue.Api.MassTransit;

public class CatalogueMassTransitService : IMassTransitService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CatalogueMassTransitService> _logger;
    public CatalogueMassTransitService(

        ILogger<CatalogueMassTransitService> logger,
        IPublishEndpoint publishEndpoint)

    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task Send<T>(T message) where T : class
    {
        await _publishEndpoint.Publish(message);
    }
}