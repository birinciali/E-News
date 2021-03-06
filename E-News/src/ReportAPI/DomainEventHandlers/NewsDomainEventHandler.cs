using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Models;
using ReportAPI.Controllers; 
using Serilog;

namespace ReportAPI.DomainEventHandlers 
{
    public class NewsDomainEventHandler : INotificationHandler<NewsDomainEvent>
    {
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        private ISendEndpoint _sendEndpoint;
        public NewsDomainEventHandler(IBus bus, IConfiguration configuration)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sendEndpoint =  _bus.GetSendEndpoint(new Uri($"{_configuration.GetSection("RabbitMqSettings:server").Value}/News")).GetAwaiter().GetResult();
        }
        public async Task Handle(NewsDomainEvent notification, CancellationToken cancellationToken)
        {
            try
            { 
                await _sendEndpoint.Send(new Message(notification.News.AgencyCode, notification.News.NewsContent , notification.News.CreatedOn, notification.News.IsActive));  
            }
            catch(Exception e) { 
                Log.Error(e.Message);
                Log.ForContext<CreateNewsCommandHandler>()
                .Error(
                    "Haber Kuyruğa atılamadı : {@notification}", JsonSerializer.Serialize(notification));
            }
        }
    }
}