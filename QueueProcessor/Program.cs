using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var w = new Worker();
            w.Go();
        }
    }

    public class Worker
    {
        private const string QueueURI = "GET YOUR QUEUE URI FROM YOUR AMQP INSTALLATION";
        private const string QueueName = "LiberatedQueue";
        private Uri mqUri;
        private ConnectionFactory mqConnectionFactory;
        private IConnection mqConnection;

        public void Go()
        {
            // Open the queue connection.
            // Note, if this was a real application we'd try to keep the connection open through
            // multiple requests.
            mqUri = new Uri(QueueURI);
            mqConnectionFactory = new ConnectionFactory
            {
                Uri = mqUri ?? throw new ArgumentException("You need to set the Queue URI setting in configuration!")
            };
            if (mqConnectionFactory == null) throw new ArgumentException("Failed to create the AQMP connection factory!");
            mqConnection = mqConnectionFactory.CreateConnection();
            if (mqConnection == null) throw new ArgumentException("Failed to create the AQMP connection!");

            var mqChannel = mqConnection.CreateModel();
            var consumer = new EventingBasicConsumer(mqChannel);
            consumer.Received += (ch, ea) =>
            {
                string messageContent = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"Received message: {messageContent}");
                Console.WriteLine("Now executing some time-consuming process.");
                Task.Delay(20000).Wait();
                Console.WriteLine("Done with time-consuming process.");
            };
            string consumerTag = mqChannel.BasicConsume(QueueName, true, consumer);

            // This is wrong.
            Task.Delay(-1).Wait();
        }
    }
}
