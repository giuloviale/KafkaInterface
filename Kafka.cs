using Confluent.Kafka;
using System;
using System.Threading.Tasks;
using Confluent.Kafka.Admin;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using WebJob.U21.InterfazAsignaciones;
using WebJob.U21.InterfazAsignaciones.Modelos;
using Api.Web.Dynamics365.Clases;

namespace Interfaz_Dynamics_365___SAP
{
    /// <summary>
    ///     This is a simple example demonstrating how to produce a message to
    ///     Confluent Cloud then read it back again.
    ///     
    ///     https://www.confluent.io/confluent-cloud/
    /// 
    ///     Confluent Cloud does not auto-create topics. You will need to use the ccloud
    ///     cli to create the dotnet-test-topic topic before running this example. The
    ///     <ccloud bootstrap servers>, <ccloud key> and <ccloud secret> parameters are
    ///     available via the confluent cloud web interface. For more information,
    ///     refer to the quick-start:
    ///
    ///     https://docs.confluent.io/current/cloud-quickstart.html
    /// </summary>
    class Kafka
    {
        ProducerConfig pConfig;
        ConsumerConfig cConfig;
        
        public Kafka()
       
        {
            #region EntornoDesarrollo
            /*pConfig = new ProducerConfig
            {
                BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                // Note: If your root CA certificates are in an unusual location you
                // may need to specify this using the SslCaLocation property.
                SaslUsername = "VT7EZ7OU7UR6ON43",
                SaslPassword = "8f8wfQo8IZBa1FfpIuYs+FPAnkue/bG6BwzJ/PbUtyhzSizo4MkxZ/iRcNz8vRQg",
            };*/
            #endregion

            #region EntornoQA
            pConfig = new ProducerConfig
           {
               BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
               SaslMechanism = SaslMechanism.Plain,
               SecurityProtocol = SecurityProtocol.SaslSsl,
               // Note: If your root CA certificates are in an unusual location you
               // may need to specify this using the SslCaLocation property.
               SaslUsername = "AUMJQEG25CMU5DGD",
               SaslPassword = "ic/ybVDN8gQ7RcH/+Nh1leydXEL7NmAsnvlQPlIeyIREJFWHhBXpwQ07hth5ZUOe",
           };     
            #endregion

            #region EntornoProductivo
           /* pConfig = new ProducerConfig
            {

                BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                // Note: If your root CA certificates are in an unusual location you
                // may need to specify this using the SslCaLocation property.
                SaslUsername = "O6TTVN5JHIL7XPFL",
                SaslPassword = "2r0mUywfGmJic1wX1xQMpa/Ho3bz4NMaiYQsfskrVEKKF7MmORg1Km5vb+pFwl2U",
            };*/
            #endregion



         
           /* cConfig = new ConsumerConfig
            {
                BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "AUMJQEG25CMU5DGD",
                SaslPassword = "ic/ybVDN8gQ7RcH/+Nh1leydXEL7NmAsnvlQPlIeyIREJFWHhBXpwQ07hth5ZUOe",
                GroupId = "hrfactor.cg.qa",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };*/
        }
        public void EnviarJsonDesdeProducer(AsignacionGeneral asignacionGeneral)
        {
            Action<DeliveryReport<Null, string>> handler = r =>
            Console.WriteLine(!r.Error.IsError
                ? $"Delivered message to {r.TopicPartitionOffset}"
                : $"Delivery Error: {r.Error.Reason}");

            using (var p = new ProducerBuilder<Null, string>(pConfig).Build())
            {


                string asignacionJson = JsonConvert.SerializeObject(asignacionGeneral);
                p.Produce("class-scheduler.teacher-assignment", new Message<Null, string> { Value = asignacionJson }, handler);//JSON
                Thread.Sleep(8000);


                // wait for up to 10 seconds for any inflight messages to be delivered.
                p.Flush(TimeSpan.FromSeconds(10));
            }
        }
            /*public void EnviarJsonDesdeProducer(List<AsignacionRol3> Asignaciones)
            {
                Action<DeliveryReport<Null, string>> handler = r =>
                Console.WriteLine(!r.Error.IsError
                    ? $"Delivered message to {r.TopicPartitionOffset}"
                    : $"Delivery Error: {r.Error.Reason}");

                using (var p = new ProducerBuilder<Null, string>(pConfig).Build())
                {
                    foreach (Asignacion asignacion in Asignaciones)
                    {
                        string asignacionJson = JsonConvert.SerializeObject(asignacion);
                        p.Produce("classs-cheduler.teacher-assignment", new Message<Null, string> { Value = asignacionJson }, handler);//JSON
                        Thread.Sleep(8000);
                    }

                    // wait for up to 10 seconds for any inflight messages to be delivered.
                    p.Flush(TimeSpan.FromSeconds(10));
                }
            }*/
            public void RecibirJsonDesdeConsumer()
        {
            using (var consumer = new ConsumerBuilder<Null, string>(cConfig).Build())
            {
                consumer.Subscribe("user.employee");

                try
                {
                    var consumeResult = consumer.Consume();
                    Console.WriteLine($"consumed: {consumeResult.Message.Value}");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"consume error: {e.Error.Reason}");
                }

                consumer.Close();
            }
        }

        /*
        public static async Task Main(string[] args)
        {
            var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            using (var p = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var dr = await p.ProduceAsync("test-topic", new Message<Null, string> { Value = "test" });
                    Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }*/
        //static void Main(string[] args)
        //{
        //JsonConvert.SerializeObject(ObjetoaSerializar);
        //using (var p = new ProducerBuilder<Null, string>(pConfig).Build())
        //{
        //    try
        //    {
        //        var dr = p.ProduceAsync("test-topic", new Message<Null, string> { Value = "test" });
        //        Console.WriteLine($"Delivered '{dr.Result.Value}' to '{dr.Result.TopicPartitionOffset}'");
        //    }
        //    catch (ProduceException<Null, string> e)
        //    {
        //        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        //    }
        //}
        /*
        string bootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092";
        string topicName = "dotnet-test-topic";

        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
        {
            try
            {
                 adminClient.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                Console.ReadKey();
            }
        }*/
        //var pConfig = new ProducerConfig
        //{
        //    BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
        //    SaslMechanism = SaslMechanism.Plain,
        //    SecurityProtocol = SecurityProtocol.SaslSsl,
        //    // Note: If your root CA certificates are in an unusual location you
        //    // may need to specify this using the SslCaLocation property.
        //    SaslUsername = "VT7EZ7OU7UR6ON43",
        //    SaslPassword = "8f8wfQo8IZBa1FfpIuYs+FPAnkue/bG6BwzJ/PbUtyhzSizo4MkxZ/iRcNz8vRQg",
        //};
        //Console.WriteLine("Creando Producer");

        //Version 1
        //Action<DeliveryReport<Null, string>> handler = r =>
        //Console.WriteLine(!r.Error.IsError
        //    ? $"Delivered message to {r.TopicPartitionOffset}"
        //    : $"Delivery Error: {r.Error.Reason}");

        //using (var p = new ProducerBuilder<Null, string>(pConfig).Build())
        //{
        //    for (int i = 0; i < 10; ++i)
        //    {
        //        p.Produce("user.employee", new Message<Null, string> { Value = i.ToString() }, handler);//JSON
        //    }

        //    // wait for up to 10 seconds for any inflight messages to be delivered.
        //    p.Flush(TimeSpan.FromSeconds(10));
        //}



        //Version 2
        //string resultado = "";
        //using (var producer = new ProducerBuilder<Null, string>(pConfig).Build())
        //{
        //    producer.ProduceAsync("user.employee", new Message<Null, string> { Value = "test value" })//Enviar un JSON
        //       .ContinueWith(task => task.IsFaulted
        //           ? resultado = $"error producing message: {task.Exception.Message}"
        //           : resultado = $"produced to: {task.Result.TopicPartitionOffset}");

        //    // block until all in-flight produce requests have completed (successfully
        //    // or otherwise) or 10s has elapsed.
        //    producer.Flush(TimeSpan.FromSeconds(10));
        //}

        //Console.WriteLine($"Resultado: {resultado}");

        //Console.ReadKey();
        //Console.ReadKey();
        //Console.ReadKey();
        /*
        var cConfig = new ConsumerConfig
        {
            BootstrapServers = "pkc-4nym6.us-east-1.aws.confluent.cloud:9092",
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = "VT7EZ7OU7UR6ON43",
            SaslPassword = "8f8wfQo8IZBa1FfpIuYs+FPAnkue/bG6BwzJ/PbUtyhzSizo4MkxZ/iRcNz8vRQg",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        Console.WriteLine("Creando Consumer");
        using (var consumer = new ConsumerBuilder<Null, string>(cConfig).Build())
        {
            consumer.Subscribe("dotnet-test-topic");

            try
            {
                var consumeResult = consumer.Consume();
                Console.WriteLine($"consumed: {consumeResult.Message.Value}");
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"consume error: {e.Error.Reason}");
            }

            consumer.Close();
            Console.ReadKey();
        }*/
        //}
    }
}
