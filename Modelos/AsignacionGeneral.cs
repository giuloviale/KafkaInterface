using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJob.U21.InterfazAsignaciones.Modelos
{
    public class AsignacionGeneral
    {
        public AsignacionGeneral(string idCatedra, string idIntegracion, List<Teachers> teachers) 
        {
            this.idCatedra = idCatedra;
            this.idIntegracion = idIntegracion;
            this.teachers = teachers;
        }

        [JsonProperty("division.new_idcatedra")]
        public string idCatedra { get; set; }

        [JsonProperty("division.new_idintegracion")]
        public string idIntegracion { get; set; }

        public List<Teachers> teachers { get; set; }
        

    }
}
