using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJob.U21.InterfazAsignaciones
{
    public class Errores
    {
        public Error error { get; set; }

        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }
    }
}
