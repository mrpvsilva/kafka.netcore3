using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Consumer.Handler.Handlers
{
    public class MessageHandler : IHostedService
    {
        private readonly ILogger _logger;
        private readonly Shared.KafakOptions _options;
        public MessageHandler(ILogger<MessageHandler> logger, IOptions<Shared.KafakOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted Service running.");

            var conf = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = _options.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe("pedido");
                var cts = new CancellationTokenSource();

                try
                {
                    while (true)
                    {
                        var message = c.Consume(cts.Token);
                        _logger.LogInformation($"Mensagem: {message.Value} recebida de {message.TopicPartitionOffset}");
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted Service is stopping.");
            return Task.CompletedTask;
        }
    }
}
