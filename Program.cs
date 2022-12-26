using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.ServiceModel.Description;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Tooling.Connector;
using Api.Web.Dynamics365.Clases;
using Newtonsoft.Json.Linq;
using static WebJob.U21.InterfazAsignaciones.Modelos.Credenciales;
using WebJob.U21.InterfazAsignaciones.Modelos;

namespace Interfaz_Dynamics_365___SAP
{
    public class CrmManager
    {
       //get the credentials




        public IOrganizationService CrmService { get => crmService; set => crmService = value; }



        private AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        {
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(_userName,
                            _password,
                            _domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials =
                        DeviceIdManager.LoadOrRegisterDevice();
                    break;
                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  // Windows Kerberos

                    // The service is configured for User Id authentication, but the user might provide Microsoft
                    // account credentials. If so, the supporting credentials must contain the device credentials.
                    if (endpointType == AuthenticationProviderType.OnlineFederation)
                    {
                        IdentityProvider provider = service.GetIdentityProvider(authCredentials.ClientCredentials.UserName.UserName);
                        if (provider != null && provider.IdentityProviderType == IdentityProviderType.LiveId)
                        {
                            authCredentials.SupportingCredentials = new AuthenticationCredentials();
                            authCredentials.SupportingCredentials.ClientCredentials =
                               DeviceIdManager.LoadOrRegisterDevice();
                        }
                    }

                    break;
            }

            return authCredentials;
        }

        /// <summary>
        /// Discovers the organizations that the calling user belongs to.
        /// </summary>
        /// <param name="service">A Discovery service proxy instance.</param>
        /// <returns>Array containing detailed information on each organization that 
        /// the user belongs to.</returns>
        public OrganizationDetailCollection DiscoverOrganizations(IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse =
                (RetrieveOrganizationsResponse)service.Execute(orgRequest);

            return orgResponse.Details;
        }

        public OrganizationDetail FindOrganization(string orgUniqueName, OrganizationDetail[] orgDetails)
        {
            if (String.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (OrganizationDetail detail in orgDetails)
            {
                if (String.Compare(detail.UniqueName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            return orgDetail;
        }


        public static void Main(string[] args)
        {
            try
            {
                //-----------------------------Para probar a peticion espero un mensaje y si lo recibo ejecuto------------------
                // var queue = MessagingFactory.CreateFromConnectionString("Endpoint=sb://sgroneclicksepyme.servicebus.windows.net/;SharedAccessKeyName=SAPcolau21empleados;SharedAccessKey=cl2bZy858Y3WJca2eXi2qFuHGTPHxq0qwv10oWrYEbA=");

                //var client = queue.CreateMessageReceiver("colau21empleados(", ReceiveMode.PeekLock);
                var queue = MessagingFactory.CreateFromConnectionString("Endpoint=sb://sgroneclicksepyme.servicebus.windows.net/;SharedAccessKeyName=saskeyinterfazasignaciones;SharedAccessKey=wAlLBwV+tREevwTq12wxvMen3QV3M0TErOi9J9wnbpo=");
                var client = queue.CreateMessageReceiver("colaus21interfazasignaciones", ReceiveMode.PeekLock);


                var message = client.Receive(new TimeSpan(0, 0, 5));
                if (message == null)
                {
                        System.Console.WriteLine("Sin Mensajes en Cola..");
                    return;
                }

                RemoteExecutionContext context = message.GetBody<RemoteExecutionContext>();

                System.Console.WriteLine("Enviado Mensaje de Cola a WebApp...");
                System.Console.WriteLine("Id Entidad: " + context.PrimaryEntityId);
                System.Console.WriteLine("Nombre Entidad: " + context.PrimaryEntityName);
                System.Console.WriteLine("Fecha Operacion: " + context.OperationCreatedOn.ToUniversalTime().ToString());

                EntityReference Division;
                client.Complete(message.LockToken);
                Division = (context.InputParameters.Contains("Division") && context.InputParameters["Division"] != null ? ((EntityReference)context.InputParameters["Division"]) : null);

                CrmManager crmManager = new CrmManager();
                crmManager._organizationUniqueName = (context.InputParameters.Contains("Nombre Organizacion") && context.InputParameters["Nombre Organizacion"] != null ? context.InputParameters["Nombre Organizacion"].ToString() : string.Empty);
                //_discoveryServiceAddress = (context.InputParameters.Contains("Url Discovery") ? context.InputParameters["Url Discovery"].ToString() : string.Empty);
                crmManager._userName = (context.InputParameters.Contains("Crm User") && context.InputParameters["Crm User"] != null ? context.InputParameters["Crm User"].ToString() : string.Empty);
                crmManager._password = (context.InputParameters.Contains("Crm Password") && context.InputParameters["Crm Password"] != null ? context.InputParameters["Crm Password"].ToString() : string.Empty);
                //if (crmManager._organizationUniqueName.Equals(string.Empty) || crmManager._discoveryServiceAddress.Equals(string.Empty) || crmManager._userName.Equals(string.Empty) || crmManager._password.Equals(string.Empty))
                //{
                //System.Console.WriteLine("Parametros vacíos para establecer la conexión...");
                //System.Console.WriteLine("Conectandose con UAT por defecto");
                crmManager._organizationUniqueName = "org47d94b4e";//Produccion //"d1cdd3456e284dd3ad33d3af30db0e";//UAT
                crmManager._userName = "power_platform@ues21.edu.ar";
                crmManager._password = "yPhkkLs3s4UZZptPRNBU";
                //return
                //}
                //--------------------------------------------------------------------------------------------------------------
                //crmManager.SetCrmService();

                crmManager._stringConnectionOAuth = (context.InputParameters.Contains("String Connection OAUth") && context.InputParameters["String Connection OAUth"] != null ? context.InputParameters["String Connection OAUth"].ToString() : string.Empty);

                Console.WriteLine(crmManager._stringConnectionOAuth);

                crmManager.SetCrmService(crmManager._stringConnectionOAuth);

                Console.WriteLine("Se establecio conexion con el CRM");

                ApiDynamics apiDynamics = new ApiDynamics();

                Guid idDivision = context.PrimaryEntityId;

                JArray divisiones = apiDynamics.BuscarDivision(apiDynamics, idDivision);

                JArray divisiones2 = apiDynamics.BuscarDivision2(apiDynamics, idDivision);

                JObject division = apiDynamics.ArmarDivision(divisiones);

                JObject division2 = apiDynamics.ArmarDivision(divisiones2);
                
                int cantProfDiscp = 0;

                int cantProfVirtual = 0;

                int cantProfCae = 0;

                int cantProfAyu = 0;

                int cantProfAds = 0;

                if (division.ContainsKey("new_cantidadprofesoresdisciplinar"))
                {
                    cantProfDiscp = (int)division.GetValue("new_cantidadprofesoresdisciplinar");
                }
                if (division.ContainsKey("new_cantidaddeprofesoresvirtual"))
                {
                    cantProfVirtual = (int)division.GetValue("new_cantidaddeprofesoresvirtual");
                }
                if (division2.ContainsKey("new_cantidaddeprofesorescae"))
                {
                    cantProfCae = (int)division2.GetValue("new_cantidaddeprofesorescae");
                }
                if (division2.ContainsKey("new_cantidadprofesorayudantes"))
                {
                    cantProfAyu = (int)division2.GetValue("new_cantidadprofesorayudantes");
                }
                if (division2.ContainsKey("new_cantidadprofesoradscriptos"))
                {
                    cantProfAds = (int)division2.GetValue("new_cantidadprofesoradscriptos");
                }

                List<Teachers> teachers = new List<Teachers>();

                if (division.GetValue("_new_profesortitularexperto_value") != null)
                {
                    teachers.Add(new Teachers(1, (int)division.GetValue("ca.new_nrodocumento")));
                    AgregarProfDisp(cantProfDiscp,division,teachers);
                    AgregarProfVirtual(cantProfVirtual, division, teachers);
                    AgregarProfCae(cantProfCae, division2, teachers);
                    AgregarProfAy(cantProfAyu, division2, teachers);
                    AgregarProfAd(cantProfAyu, division2, teachers);
                }
                else
                {
                    AgregarProfDisp(cantProfDiscp, division, teachers);
                    AgregarProfVirtual(cantProfVirtual, division, teachers);
                    AgregarProfCae(cantProfCae, division2, teachers);
                    AgregarProfAy(cantProfAyu, division2, teachers);
                    AgregarProfAd(cantProfAyu, division2, teachers);
                }

                AsignacionGeneral asignacionGeneral = new AsignacionGeneral(division.GetValue("new_idcatedra").ToString(), null, teachers);
                Kafka kafka = new Kafka();
                kafka.EnviarJsonDesdeProducer(asignacionGeneral);
                string asignacionJson = JsonConvert.SerializeObject(asignacionGeneral);
                apiDynamics.CreateRecord("new_controlkafkas", apiDynamics.ArmarControlKafka(asignacionJson));

            }
            catch (Exception e)
            {
                string Error = e.ToString();
                Console.WriteLine(Error);
                throw;
            }
        }


        public void SetCrmService(string ConnectionString)
        {
            try
            {
                //crmService = organizationProxy;
                //string ConnectionString = "AuthType = OAuth; " +
                //"Username = sgradmin@trendsgr.onmicrosoft.com;" +
                //"Password = Sgr.2020$; " +
                //"Url = https://trendsgr.crm2.dynamics.com;" +
                //"AppId=f74c3e38-c83f-4515-84ba-ae23661691f9;" +
                //"RedirectUri=app://sgrauthenticationapp.com;" +
                //"LoginPrompt=Auto";

                CrmService = new CrmServiceClient(ConnectionString);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Excepcion en SetCrmService():", e.Message);
            }
        }
        public TProxy GetProxy<TService, TProxy>(
            IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }
        public void SetCrmService()
        {
            try
            {
                //if (this.crmService != null) return;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                IServiceManagement<IDiscoveryService> serviceManagement =
                       ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                       new Uri(_discoveryServiceAddress));
                AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

                // Set the credentials.
                AuthenticationCredentials authCredentials = GetCredentials(serviceManagement, endpointType);


                String organizationUri = String.Empty;
                // Get the discovery service proxy.
                using (DiscoveryServiceProxy discoveryProxy =
                    GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
                {
                    // Obtain organization information from the Discovery service. 
                    if (discoveryProxy != null)
                    {
                        // Obtain information about the organizations that the system user belongs to.
                        OrganizationDetailCollection orgs = DiscoverOrganizations(discoveryProxy);
                        // Obtains the Web address (Uri) of the target organization.
                        organizationUri = FindOrganization(_organizationUniqueName,
                            orgs.ToArray()).Endpoints[EndpointType.OrganizationService];

                    }
                }

                if (!String.IsNullOrWhiteSpace(organizationUri))
                {
                    IServiceManagement<IOrganizationService> orgServiceManagement =
                        ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
                        new Uri(organizationUri));

                    // Set the credentials.
                    AuthenticationCredentials credentials = GetCredentials(orgServiceManagement, endpointType);

                    // Get the organization service proxy.
                    using (OrganizationServiceProxy organizationProxy =
                        GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, credentials))
                    {
                        // This statement is required to enable early-bound type support.
                        organizationProxy.EnableProxyTypes();

                        organizationProxy.Timeout = new TimeSpan(0, 5, 0);

                        CrmService = organizationProxy;
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Excepcion en SetCrmService():", e.Message);

            }
        }
        public static void AgregarProfDisp(int cantProfDiscp,JObject division, List<Teachers> teachers)
        {
            switch (cantProfDiscp)
            {
                case 0:
                    break;
                case 100000000:
                    teachers.Add(new Teachers(2, (int)division.GetValue("ba.new_nrodocumento")));
                    break;
                case 100000001:
                    teachers.Add(new Teachers(2, (int)division.GetValue("ba.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bb.new_nrodocumento")));
                    break;
                case 100000002:
                    teachers.Add(new Teachers(2, (int)division.GetValue("ba.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bb.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bc.new_nrodocumento")));
                    break;
                case 100000003:
                    teachers.Add(new Teachers(2, (int)division.GetValue("ba.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bb.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bc.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bd.new_nrodocumento")));
                    break;
                case 100000004:
                    teachers.Add(new Teachers(2, (int)division.GetValue("ba.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bb.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bc.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("bd.new_nrodocumento")));
                    teachers.Add(new Teachers(2, (int)division.GetValue("be.new_nrodocumento")));
                    break;
            }
        }
        public static void AgregarProfVirtual(int cantProfVirtual, JObject division, List<Teachers> teachers)
        {
            switch (cantProfVirtual)
            {
                case 0:
                    break;
                case 100000000:
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    break;
                case 100000001:
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ab.new_nrodocumento")));
                    break;
                case 100000002:
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ac.new_nrodocumento")));
                    break;
                case 100000003:
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ab.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ac.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ad.new_nrodocumento")));
                    break;
                case 100000004:
                    teachers.Add(new Teachers(3, (int)division.GetValue("aa.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ab.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ac.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ad.new_nrodocumento")));
                    teachers.Add(new Teachers(3, (int)division.GetValue("ae.new_nrodocumento")));
                    break;

            }
        }
        public static void AgregarProfCae(int cantProfCae, JObject division2, List<Teachers> teachers)
        {
            switch (cantProfCae)
            {
                case 0:
                    break;
                case 100000000:
                    teachers.Add(new Teachers(6, (int)division2.GetValue("da.new_nrodocumento")));
                    break;
                case 100000001:
                    teachers.Add(new Teachers(6, (int)division2.GetValue("da.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("db.new_nrodocumento")));
                    break;
                case 100000002:
                    teachers.Add(new Teachers(6, (int)division2.GetValue("da.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("db.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("dc.new_nrodocumento")));
                    break;
                case 100000003:
                    teachers.Add(new Teachers(6, (int)division2.GetValue("da.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("db.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("dc.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("dd.new_nrodocumento")));
                    break;
                case 100000004:
                    teachers.Add(new Teachers(6, (int)division2.GetValue("da.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("db.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("dc.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("dd.new_nrodocumento")));
                    teachers.Add(new Teachers(6, (int)division2.GetValue("de.new_nrodocumento")));
                    break;
            }
        }
        public static void AgregarProfAy(int cantProfAy, JObject division2, List<Teachers> teachers)
        {
            switch (cantProfAy)
            {
                case 0:
                    break;
                case 100000000:
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ea.new_nrodocumento")));
                    break;
                case 100000001:
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ea.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("eb.new_nrodocumento")));
                    break;
                case 100000002:
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ea.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("eb.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ec.new_nrodocumento")));
                    break;
                case 100000003:
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ea.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("eb.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ec.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ed.new_nrodocumento")));
                    break;
                case 100000004:
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ea.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("eb.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ec.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ed.new_nrodocumento")));
                    teachers.Add(new Teachers(7, (int)division2.GetValue("ee.new_nrodocumento")));
                    break;
            }
        }
        public static void AgregarProfAd(int cantProfAd, JObject division2, List<Teachers> teachers)
        {
            switch (cantProfAd)
            {
                case 0:
                    break;
                case 100000000:
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fa.new_nrodocumento")));
                    break;
                case 100000001:
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fa.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fb.new_nrodocumento")));
                    break;
                case 100000002:
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fa.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fb.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fc.new_nrodocumento")));
                    break;
                case 100000003:
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fa.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fb.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fc.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fd.new_nrodocumento")));
                    break;
                case 100000004:
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fa.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fb.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fc.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fd.new_nrodocumento")));
                    teachers.Add(new Teachers(8, (int)division2.GetValue("fe.new_nrodocumento")));
                    break;
            }
        }


    }
}
