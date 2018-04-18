using Olsa;
using Olsa.WCF.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace SkillsoftReportConsole.Helpers
{
    public static class Olsa
    {
        /// <summary>
        /// Gets the olsa client
        /// </summary>
        /// <param name="olsaServerEndpoint">The olsa server endpoint.</param>
        /// <param name="olsaCustomerId">The olsa customer identifier.</param>
        /// <param name="olsaSharedSecret">The olsa shared secret.</param>
        /// <returns></returns>
        public static OlsaPortTypeClient GetOLSAClient(Uri olsaServerEndpoint, string olsaCustomerId, string olsaSharedSecret)
        {
            //Set the encoding to SOAP 1.1, Disable Addressing and set encoding to UTF8
            TextMessageEncodingBindingElement messageEncoding = new TextMessageEncodingBindingElement();
            messageEncoding.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
            messageEncoding.WriteEncoding = Encoding.UTF8;

            //Setup Binding Elemment
            HttpTransportBindingElement transportBinding = new HttpsTransportBindingElement();

            //Set the maximum received messages sizes to 1Mb
            transportBinding.MaxReceivedMessageSize = 1024 * 1024;
            transportBinding.MaxBufferPoolSize = 1024 * 1024;

            //Create the CustomBinding
            Binding customBinding = new CustomBinding(messageEncoding, transportBinding);

            //Create the OLSA Service
            EndpointAddress serviceAddress = new EndpointAddress(olsaServerEndpoint);

            //Set the endPoint URL YOUROLSASERVER/olsa/services/Olsa has to be HTTPS
            OlsaPortTypeClient service = new OlsaPortTypeClient(customBinding, serviceAddress);

            //Add Behaviour to support SOAP UserNameToken Password Digest
            AuthenticationBehavior behavior1 = new AuthenticationBehavior(olsaCustomerId, olsaSharedSecret);
            service.Endpoint.Behaviors.Add(behavior1);

            //Add Behaviour to support fix of Namespaces to address AXIS2 / VWCF incompatability
            NameSpaceFixUpBehavior behavior2 = new NameSpaceFixUpBehavior();
            service.Endpoint.Behaviors.Add(behavior2);

            return service;
        }

        /// <summary>
        /// Pollfors the report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <param name="client">The client.</param>
        /// <param name="closeclient">if set to <c>true</c> [closeclient].</param>
        /// <returns></returns>
        public static UrlResponse PollforReport(string reportId, OlsaPortTypeClient client, bool closeclient = false)
        {
            //Set up our response object
            UrlResponse response = null;

            try
            {
                //Create our request
                PollForReportRequest request = new PollForReportRequest();

                //Pull the OlsaAuthenticationBehviour so we can extract the customerid
                AuthenticationBehavior olsaCredentials = (AuthenticationBehavior)client.ChannelFactory.Endpoint.Behaviors.Where(p => p.GetType() == typeof(AuthenticationBehavior)).FirstOrDefault();
                request.customerId = olsaCredentials.UserName;

                request.reportId = reportId;

                response = client.UTIL_PollForReport(request);
            }
            catch (WebException)
            {
                // This captures any Web Exceptions such as proxy errors etc
                // See http://msdn.microsoft.com/en-us/library/48ww3ee9(VS.80).aspx
                throw;
            }
            catch (TimeoutException)
            {
                //This captures the WCF timeout exception
                throw;
            }
            //WCF fault exception will be thrown for any other issues such as Security
            catch (FaultException fe)
            {
                if (fe.Message.ToLower(CultureInfo.InvariantCulture).Contains("the security token could not be authenticated or authorized"))
                {
                    //The OLSA Credentials specified could not be authenticated
                    throw;
                }
                throw;
            }
            catch (Exception)
            {
                //Any other type of exception, perhaps out of memory
                throw;
            }
            finally
            {
                //Shutdown and dispose of the client
                if (client != null)
                {
                    if (client.State == CommunicationState.Faulted)
                    {
                        client.Abort();
                    }
                    if (closeclient)
                    {
                        //We cannot resue client if we close
                        client.Close();
                    }
                }
            }
            return response;
        }


        /// <summary>
        /// Initiates the full course listing.
        /// </summary>
        /// <param name="scopingUser">The username for scoping the report if report uses it</param>
        /// <param name="format">The format</param>
        /// <param name="report">The report name</param>
        /// <param name="reportParams">The report parameters.</param>
        /// <param name="language">The language, or NULL</param>
        /// <param name="duration">The duration to retain report (Use one of the following numeric values: 1 - 1 hour, 2 - 8 hours, 3 - 24 hours.</param>
        /// <param name="client">The client.</param>
        /// <param name="closeclient">if set to <c>true</c> [closeclient].</param>
        /// <returns></returns>
        public static HandleResponse SubmitReport(string scopingUser, reportFormat format, string report, MapItem[] reportParams, string language, int duration, OlsaPortTypeClient client, bool closeclient = false)
        {
            //Set up our response object
            HandleResponse response = null;

            try
            {
                //Create our request
                SubmitReportRequest request = new SubmitReportRequest();

                //Pull the OlsaAuthenticationBehviour so we can extract the customerid
                AuthenticationBehavior olsaCredentials = (AuthenticationBehavior)client.ChannelFactory.Endpoint.Behaviors.Where(p => p.GetType() == typeof(AuthenticationBehavior)).FirstOrDefault();
                request.customerId = olsaCredentials.UserName;

                request.scopingUserId = scopingUser;
                request.reportFormat = format;
                request.report = report;
                request.language = language;
                request.duration = duration;
                request.reportParameters = reportParams;


                response = client.UD_SubmitReport(request);
            }
            catch (WebException)
            {
                // This captures any Web Exceptions such as proxy errors etc
                // See http://msdn.microsoft.com/en-us/library/48ww3ee9(VS.80).aspx
                throw;
            }
            catch (TimeoutException)
            {
                //This captures the WCF timeout exception
                throw;
            }
            //WCF fault exception will be thrown for any other issues such as Security
            catch (FaultException fe)
            {
                if (fe.Message.ToLower(CultureInfo.InvariantCulture).Contains("the security token could not be authenticated or authorized"))
                {
                    //The OLSA Credentials specified could not be authenticated
                    throw;
                }
                throw;
            }
            catch (Exception)
            {
                //Any other type of exception, perhaps out of memory
                throw;
            }
            finally
            {
                //Shutdown and dispose of the client
                if (client != null)
                {
                    if (client.State == CommunicationState.Faulted)
                    {
                        client.Abort();
                    }
                    if (closeclient)
                    {
                        //We cannot resue client if we close
                        client.Close();
                    }
                }
            }
            return response;

        }


    }
}
