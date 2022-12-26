using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJob.U21.InterfazAsignaciones
{
    internal class Excepciones
    {
        private string cuit;
        private string v1;
        private string v2;

        public Excepciones()
        {

        }
        public Excepciones(string id, string v1, string v2)
        {

            this.v1 = v1;
            this.v2 = v2;
        }
        public Excepciones(string id, string v1)
        {

            this.v1 = v1;
           
        }
        public Error error { get; set; }
        //public Excepcion excepcion { get; set; }

        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
            public string campo { get; set; }
        }
    }
}
