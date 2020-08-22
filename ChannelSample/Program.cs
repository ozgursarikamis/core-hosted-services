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

        public static async Task MultiProducerSingleConsumer()
        {
            var channel = Channel.CreateUnbounded<string>();

            // In this example, a single consumer easily keeps up with two producers

            var producer1 = new Producer(channel.Writer, 1, 2000);
            var producer2 = new Producer(channel.Writer, 2, 2000);
            var consumer1 = new Consumer(channel.Reader, 1, 250);

            Task consumerTask1 = consumer1.ConsumeData(); // begin consuming

            Task producerTask1 = producer1.BeginProducing();

            await Task.Delay(500); // stagger the producers

            Task producerTask2 = producer2.BeginProducing();

            await Task.WhenAll(producerTask1, producerTask2)
                .ContinueWith(_ => channel.Writer.Complete());

            // Writer.Complete() Marks the channel as being complete
            // no more items will be written to it

            await consumerTask1;
        }

        public static async Task SingleProduceMultipleConsumers()
        {
            var channel = Channel.CreateUnbounded<string>();
            // In this example, multiple consumers are needed to keep up with a fast producer

            var producer1 = new Producer(channel.Writer, 1, 100);
            var consumer1 = new Consumer(channel.Reader, 1, 1500);
            var consumer2 = new Consumer(channel.Reader, 2, 1500);
            var consumer3 = new Consumer(channel.Reader, 3, 1500);

            Task consumerTask1 = consumer1.ConsumeData();
            Task consumerTask2 = consumer2.ConsumeData();
            Task consumerTask3 = consumer3.ConsumeData();

            Task producerTask1 = producer1.BeginProducing();

            await producerTask1.ContinueWith(_ => channel.Writer.Complete());

            await Task.WhenAll(consumerTask1, consumerTask2, consumerTask3);
        }
    }
}
