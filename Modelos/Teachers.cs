using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WebJob.U21.InterfazAsignaciones.Modelos
{
    public class Teachers
    {
        public Teachers(int roleId, int dni)
        {
            this.roleId = roleId;
            this.dni = dni;
        }

        public int roleId { get; set; }


        [JsonProperty("docente.new_nrodocumento")]
        public int dni { get; set; }
    }
}
