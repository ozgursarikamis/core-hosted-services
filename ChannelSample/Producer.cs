using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelSample
{
    internal class Producer
    {
        private readonly ChannelWriter<string> _writer;
        private readonly int _identifier;
        private readonly int _delay;

        public Producer(ChannelWriter<string> writer, int identifier, int delay)
        {
            _writer = writer;
            _identifier = identifier;
            _delay = delay;
        }

        public async Task BeginProducing()
        {
            Console.WriteLine($"PRODUCER ({_identifier}): Starting");

            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(_delay); // simulate producer building/fetching some data

                var msg = $"P{_identifier} - {DateTime.UtcNow:G}";

                Console.WriteLine($"PRODUCER ({_identifier}): Creating {msg}");

                await _writer.WriteAsync(msg);
            }

            Console.WriteLine($"PRODUCER ({_identifier}): Completed");
        }
    }
}
