using CommandLine;
using Olsa;
using Olsa.WCF.Extensions;
using SkillsoftReportConsole.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace SkillsoftReportConsole
{

    class Program
    {
        static void Process(Uri endpoint, string customerId, string sharedSecret)
        {
            //Create the OLSA Web Services Client using code
            OlsaPortTypeClient client = Helpers.Olsa.GetOLSAClient(endpoint, customerId, sharedSecret);

            Console.WriteLine("Issuing the UD_SubmitReport Request");
            Console.WriteLine("------------------------------------");
            HandleResponse handleResponse = new HandleResponse();

            //--------------------------------------------------------------------------------------------
            //Define report settings here to ease changes
            string scopingUserId = "admin";
            reportFormat reportFormat = reportFormat.CSV;
            string reportFilename = "report." + reportFormat.ToString();

            string reportLanguage = "en_US"; //en_US only supported value
            int reportRetainperiod = 3; //1 - 1 hour, 2 - 8 hours, 3 - 24 hours.

            //For details of report names and parameters see https://documentation.skillsoft.com/en_us/skillport/8_0/ah/35465.htm
            string reportName = "summary_catalog";
            //Define report parameters
            List<MapItem> paramList = new List<MapItem>();
            paramList.Add(new MapItem() { key = "asset_category", value = "1,2,3,4,5,21" });
            paramList.Add(new MapItem() { key = "display_options", value = "all" });


            //--------------------------------------------------------------------------------------------

            //Submit Report request
            try
            {
                handleResponse = Helpers.Olsa.SubmitReport(scopingUserId, reportFormat, reportName, paramList.ToArray(), reportLanguage, reportRetainperiod, client, false);

                //Minutes between polls
                int sleepInterval = 2;
                Console.WriteLine("Handle: {0}", handleResponse.handle);

                UrlResponse url = null;

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("{0}: Sleeping {1} minutes", DateTime.UtcNow.ToLongTimeString(), sleepInterval);
                    //ms - so here we sleep for 10 minutes
                    System.Threading.Thread.Sleep(sleepInterval * 60 * 1000);

                    try
                    {
                        url = Helpers.Olsa.PollforReport(handleResponse.handle, client, false);
                        break;
                    }
                    catch (FaultException<Olsa.DataNotReadyYetFault>)
                    {
                        //The report has not completed generation, we are checking for a Specific OLSA Exception
                        Console.WriteLine("{0}: The specified report is not yet ready", DateTime.UtcNow.ToLongTimeString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0}: Issue while Polling for Report.", DateTime.UtcNow.ToLongTimeString());
                        Console.WriteLine("{0}: Exception: {1}", DateTime.UtcNow.ToLongTimeString(), ex.ToString());
                        throw;
                    }
                }

                if (url != null)
                {
                    WebClient myWebClient = new WebClient();
                    Console.WriteLine("{0}: Downloading Report: {1}", DateTime.UtcNow.ToLongTimeString(), url.olsaURL);
                    myWebClient.DownloadFile(url.olsaURL, reportFilename);
                    Console.WriteLine("{0}: Successfully Downloaded: {1}", DateTime.UtcNow.ToLongTimeString(), reportFilename);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: Issue while Submitting Report.", DateTime.UtcNow.ToLongTimeString());
                Console.WriteLine("{0}: Exception: {1}", DateTime.UtcNow.ToLongTimeString(), ex.ToString());
            }
            //Now we can close the client
            if (client != null)
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }

                client.Close();
            }
        }


        static void Main(string[] args)
        {
            Helpers.Networking.EnableTLS12();

            //Specify a default Web Proxy for all System.NET WebRequests, by calling
            //SetDefaultProxy(String proxyserver, String proxyusername, String proxypassword, String proxydomain)

            if (!Parser.TryParse(args, out Options options))
            {
                return;
            }
            try
            {
                Process(new Uri(options.Endpoint), options.CustomerId, options.SharedSecret);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: Issue while Processing.", DateTime.UtcNow.ToLongTimeString());
                Console.WriteLine("{0}: Exception: {1}", DateTime.UtcNow.ToLongTimeString(), ex.ToString());
            }
        }
    }
}

