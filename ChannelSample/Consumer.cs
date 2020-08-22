using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelSample
{
    internal class Consumer
    {
        private readonly ChannelReader<string> _reader;
        private readonly int _identifier;
        private readonly int _delay;

        public Consumer(ChannelReader<string> reader, int identifier, int delay)
        {
            _reader = reader;
            _identifier = identifier;
            _delay = delay;
        }

        public async Task ConsumeData()
        {
            Console.WriteLine($"CONSUMER ({_identifier}): Starting");

            while (await _reader.WaitToReadAsync())
            {
                if (_reader.TryRead(out var timeString))
                {
                    await Task.Delay(_delay); // simulate processing time

                    Console.WriteLine($"CONSUMER ({_identifier}): Consuming {timeString}");
                }
            }

            Console.WriteLine($"CONSUMER ({_identifier}): Completed");
        }
    }
}
