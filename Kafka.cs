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
   
    class Kafka
    {
        ProducerConfig pConfig;
        ConsumerConfig cConfig;
        
        public Kafka()
       
        {
            
            
            pConfig = new ProducerConfig
           {
               BootstrapServers = "",
               SaslMechanism = SaslMechanism.Plain,
               SecurityProtocol = SecurityProtocol.SaslSsl,
               // Note: If your root CA certificates are in an unusual location you
               // may need to specify this using the SslCaLocation property.
               SaslUsername = "",
               SaslPassword = "",
           };     
           
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
                p.Produce("", new Message<Null, string> { Value = asignacionJson }, handler);//JSON
                Thread.Sleep(8000);


                // wait for up to 10 seconds for any inflight messages to be delivered.
                p.Flush(TimeSpan.FromSeconds(10));
            }
        }
            
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


    }
}
