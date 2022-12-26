using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJob.U21.InterfazAsignaciones.Modelos
{
    public class Credenciales
    {
        public Credencial credencial { get; set; }
        public string clientid { get; internal set; } //esta oauth
        public string clientsecret { get; internal set; } //esta oauth
        public string tenantid { get; internal set; } //me lo pasa nico
        public string url { get; internal set; }
        public string cliente { get; internal set; }

        public class Credencial
        {

        }
    }
}
