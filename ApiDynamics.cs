using Microsoft.Extensions.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Services.Description;
using System.Windows.Documents;
using WebJob.U21.InterfazAsignaciones;
using static Api.NETCORE.Dynamics365.Clases.ApiService;
using WebJob.U21.InterfazAsignaciones.Modelos;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Api.Web.Dynamics365.Clases
{
    public class ApiDynamics
    {

        public string EntityName { get; set; }
        public string Attributes { get; set; }
        public string Filter { get; set; }
        public string FetchXML { get; set; }
        IOrganizationService Service;
        Errores excepciones = new Errores();
        public JArray RetrieveMultipleWithFetch(ApiDynamics api)
        {

            //string[] connectionString;
            string connectionString;
            string userName;
            string password;
            string serviceUrl;
            string clientId;
            string redirectUrl;
            string consulta = string.Empty;
            JArray body = new JArray();


            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            string Entidad = api.EntityName;
            try
            {
                connectionString = "sarasa";
                //Obtener connection string para las credenciales de Autenticacion 
                //connectionString = (builder.Configuration.GetConnectionString("ConnectionStringUAT") != null) ? builder.Configuration.GetConnectionString("ConnectionStringUAT").Split(';') : null;
                if (connectionString != null)
                {


                    // userName = connectionString[1].Remove(0, 11);
                    userName = "power_platform@ues21.edu.ar";
                    //password = connectionString[2].Remove(0, 11);
                    password = "yPhkkLs3s4UZZptPRNBU";
                    //serviceUrl = connectionString[3].Remove(0, 6);
                    serviceUrl = "https://hrfactorsues21eduaruat.crm2.dynamics.com";
                    //clientId = connectionString[4].Remove(0, 8);
                    clientId = "2dd31b38-4d28-4aeb-9cf5-dc9fa10833ec";
                    //redirectUrl = connectionString[5].Remove(0, 14);
                    redirectUrl = "https://hrfactorsues21eduaruat.crm2.dynamics.com";

                    messageHandler = new OAuthMessageHandler(serviceUrl, clientId, redirectUrl, userName, password, new HttpClientHandler());

                    //Crear mensaje HTTP client para enviar peticion al CRM Web service.  
                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        //Especificar la direccion Web API del servicio y el periodo de tiempo en que se debe ejecutar.  
                        client.BaseAddress = new Uri(serviceUrl);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                        //   client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                        client.DefaultRequestHeaders.Add("charset", "utf-8");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        consulta += api.EntityName;
                        if (api.FetchXML != null) consulta += $"?fetchXml={api.FetchXML}";
                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Headers.Add("Prefer", "odata.nextLink=true");

                        mesaage = client.SendAsync(createRequest).Result;

                        if (mesaage.IsSuccessStatusCode)
                        {
                            JObject respuesta = JObject.Parse(mesaage.Content.ReadAsStringAsync().Result);
                            var valor = respuesta["value"];
                            string resultado = JsonConvert.SerializeObject(valor);
                            body = JsonConvert.DeserializeObject<dynamic>(resultado);
                            //body= JArray.Parse(resultado);   



                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);
                            if (excepciones != null)
                            {
                                error = excepciones.error.message;
                            }
                            else
                            {
                                error = "error en create";
                            }
                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones("Error en RetrieveMultipleFetch de la entidad: " + EntityName + "| Descripción: " + ex.Message, "OneClickSgr");
            }

            return body;
        }
        public string CreateRecord(string entityName, JObject entity)
        {
            string userName;
            string password;
            string serviceUrl;
            string clientId;
            string redirectUrl;
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            string id = string.Empty;

            try
            {
                string connection = "sarasa";

                if (connection != null)
                {
                    // userName = connectionString[1].Remove(0, 11);
                    userName = "power_platform@ues21.edu.ar";
                    //password = connectionString[2].Remove(0, 11);
                    password = "yPhkkLs3s4UZZptPRNBU";
                    //serviceUrl = connectionString[3].Remove(0, 6);
                    serviceUrl = "https://hrfactorsues21eduaruat.crm2.dynamics.com";
                    //clientId = connectionString[4].Remove(0, 8);
                    clientId = "2dd31b38-4d28-4aeb-9cf5-dc9fa10833ec";
                    //redirectUrl = connectionString[5].Remove(0, 14);
                    redirectUrl = "https://hrfactorsues21eduaruat.crm2.dynamics.com";

                    messageHandler = new OAuthMessageHandler(serviceUrl, clientId, redirectUrl, userName, password, new HttpClientHandler());
                    //Creamos un HTTP Client para mandar un requess message al CRM Web service
                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        //Specify the Web API address of the service and the period of time each request   
                        //has to execute.  
                        client.BaseAddress = new Uri(serviceUrl);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, $"api/data/v9.0/{entityName}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                        mesaage = client.SendAsync(createRequest).Result;

                        if (mesaage.IsSuccessStatusCode)
                        {
                            string uri = mesaage.Headers.Location.AbsoluteUri;
                            string[] uriSplit = uri.Split('(');
                            id = uriSplit[1].Replace(')', ' ').Trim();
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);
                            if (excepciones != null)
                            {
                                error = excepciones.error.message;
                            }
                            else
                            {
                                error = "error en create";
                            }
                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones("Error ante la creación de la entidad: " + EntityName + "| Descripción: " + ex.Message, "OneClickSgr");
                return "ERROR";
            }

            return id;
        }
        public JArray BuscarDivision2(ApiDynamics api, Guid id)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                api.EntityName = "new_divisions";

                fetchXML = "<fetch mapping='logical'>" +
                              "<entity name='new_division'>" +
                                "<attribute name='new_divisionid' />" +
                                "<attribute name='new_profesorcae' />" +
                                "<attribute name='new_profesorcae2' />" +
                                "<attribute name='new_profesorcae3' />" +
                                "<attribute name='new_profesorcae4' />" +
                                "<attribute name='new_profesorcae5' />" +
                                "<attribute name='new_profesorayudantes' />" +
                                "<attribute name='new_profesorayudantes2' />" +
                                "<attribute name='new_profesorayudantes3' />" +
                                "<attribute name='new_profesorayudantes4' />" +
                                "<attribute name='new_profesorayudantes5' />" +
                                "<attribute name='new_profesoradscriptos' />" +
                                "<attribute name='new_profesoradscriptos2' />" +
                                "<attribute name='new_profesoradscriptos3' />" +
                                "<attribute name='new_profesoradscriptos4' />" +
                                "<attribute name='new_profesoradscriptos5' />" +
                                "<attribute name='new_cantidaddeprofesorescae' />" +
                                "<attribute name='new_cantidadprofesoradscriptos' />" +
                                "<attribute name='new_cantidadprofesorayudantes' />" +
                                "<filter type='and'>" +
                                  $"<condition attribute='new_divisionid' operator='eq' uitype='new_division' value='{id}' />" +
                                "</filter>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorcae' link-type='outer' alias='da'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorcae2' link-type='outer' alias='db'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorcae3' link-type='outer' alias='dc'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorcae4' link-type='outer' alias='dd'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorcae5' link-type='outer' alias='de'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorayudantes' link-type='outer' alias='ea'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorayudantes2' link-type='outer' alias='eb'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorayudantes3' link-type='outer' alias='ec'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorayudantes4' link-type='outer' alias='ed'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesorayudantes5' link-type='outer' alias='ee'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesoradscriptos' link-type='outer' alias='fa'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesoradscriptos2' link-type='outer' alias='fb'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesoradscriptos3' link-type='outer' alias='fc'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesoradscriptos4' link-type='outer' alias='fd'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesoradscriptos5' link-type='outer' alias='fe'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                              "</entity>" +
                            "</fetch>";

                if (api.EntityName != string.Empty)
                {
                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlDecode(fetchXML);
                    }
                    respuesta = api.RetrieveMultipleWithFetch(api);
                }
                return respuesta;
            }
            catch (Exception)
            {

                throw;
            }
        }




        public JArray BuscarDivision(ApiDynamics api, Guid id)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                api.EntityName = "new_divisions";

                fetchXML = "<fetch mapping='logical'>" +
                              "<entity name='new_division'>" +
                                "<attribute name='new_divisionid' />" +
                                "<attribute name='new_name' />" +
                                "<attribute name='new_idcatedra' />" +
                                "<attribute name='createdon' />" +
                                "<attribute name='new_tutorvirtual5' />" +
                                "<attribute name='new_tutorvirtual4' />" +
                                "<attribute name='new_tutorvirtual3' />" +
                                "<attribute name='new_tutorvirtual2' />" +
                                "<attribute name='new_tutorvirtual' />" +
                                "<attribute name='new_profesortitularexperto' />" +
                                "<attribute name='new_profesortitulardisciplinar5' />" +
                                "<attribute name='new_profesortitulardisciplinar4' />" +
                                "<attribute name='new_profesortitulardisciplinar2' />" +
                                "<attribute name='new_profesortitulardisciplinar' />" +
                                "<attribute name='new_new_profesortitulardisciplinar3' />" +
                                "<attribute name='new_cantidadprofesoresdisciplinar' />" +
                                "<attribute name='new_cantidaddeprofesoresvirtual' />" +
                                "<filter type='and'>" +
                                  $"<condition attribute='new_divisionid' operator='eq' uitype='new_division' value='{id}' />" +
                                "</filter>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_tutorvirtual' link-type='outer' alias='aa'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_tutorvirtual2' link-type='outer' alias='ab'>" +
                                     "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_tutorvirtual3' link-type='outer' alias='ac'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_tutorvirtual4' link-type='outer' alias='ad'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_tutorvirtual5' link-type='outer' alias='ae'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesortitulardisciplinar' link-type='outer' alias='ba'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesortitulardisciplinar2' link-type='outer' alias='bb'>" +
                                     "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_new_profesortitulardisciplinar3' link-type='outer' alias='bc'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesortitulardisciplinar4' link-type='outer' alias='bd'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesortitulardisciplinar5' link-type='outer' alias='be'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_profesortitularexperto' link-type='outer' alias='ca'>" +
                                    "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                              "</entity>" +
                            "</fetch>";

                if (api.EntityName != string.Empty)
                {
                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlDecode(fetchXML);
                    }
                    respuesta = api.RetrieveMultipleWithFetch(api);
                }
                return respuesta;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public JArray BuscarAsignacionesEnUni(ApiDynamics api)
        {
            try
            {
                string fechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
                JArray respuesta = null;
                string fetchXML = string.Empty;
                api.EntityName = "new_aceptaciondedivisions";
                fetchXML = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                              "<entity name='new_aceptaciondedivision'>" +
                                "<attribute name='new_name' />" +
                                "<attribute name='statuscode'/>" +
                                "<attribute name='createdon' />" +
                                "<attribute name='new_aceptaciondedivisionid' />" +
                                "<order attribute='new_name' descending='false' />" +
                                "<filter type='and'>" +
                                  "<condition attribute='statuscode' operator='eq' value='100000000' />" +
                                "</filter>" +
                                "<link-entity name='new_empleado' from='new_empleadoid' to='new_docente' link-type='inner' alias='docente'>" +
                                  "<attribute name='new_nrodocumento' />" +
                                "</link-entity>" +
                                "<link-entity name='new_division' from='new_divisionid' to='new_division' link-type='inner' alias='division'>" +
                                  "<attribute name='new_idintegracion' />" +
                                  "<attribute name='new_idcatedra' />" +
                                  "<filter type='and'>" +
                                    "<condition attribute='statuscode' operator='eq' value='1' />" +
                                  "</filter>" +
                                  "<link-entity name='new_periodo' from='new_periodoid' to='new_periodo' link-type='inner' alias='ad'>" +
                                    "<filter type='and'>" +
                                      $"<condition attribute='new_fechadesde' operator='on-or-after' value='{fechaHoy}' />" +
                                    "</filter>" +
                                  "</link-entity>" +
                                "</link-entity>" +
                              "</entity>" +
                            "</fetch>";


                if (api.EntityName != string.Empty)
                {
                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlDecode(fetchXML);
                    }
                    respuesta = api.RetrieveMultipleWithFetch(api);
                }
                return respuesta;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<Teachers> ArmarTeachers(List<Teachers> listaTeachers)
        {
            List<Teachers> lista = JsonConvert.DeserializeObject<List<Teachers>>(listaTeachers.ToString());
            return lista;
        }
        public JObject ArmarDivision(JArray div)
        {
            JObject division;
            division = JsonConvert.DeserializeObject<JArray>(div.ToString()).ToObject<List<JObject>>().FirstOrDefault();
            return division;
        }

        public JObject ArmarControlKafka(string MensajeJson)
        {
            JObject ControlKafka = new JObject();
            ControlKafka.Add("new_mensaje", MensajeJson);
            ControlKafka.Add("new_fechadecreacin", DateTime.Today.ToString("yyyy-MM-dd"));

            return ControlKafka;
        }
    }
}
