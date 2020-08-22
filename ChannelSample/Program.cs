using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelSample
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
        }

        public static async Task SingleProducerSingleConsumer()
        {
            var channel = Channel.CreateUnbounded<string>();
            // In this example, the consumer keeps up with the producer

            var producer1 = new Producer(channel.Writer, 1, 2000);
            var consumer1 = new Consumer(channel.Reader, 1, 1500);

            Task consumerTask1 = consumer1.ConsumeData();
            Task producerTask1 = producer1.BeginProducing();

            await producerTask1.ContinueWith(_ => channel.Writer.Complete());
            await consumerTask1;
        }
    }
}
