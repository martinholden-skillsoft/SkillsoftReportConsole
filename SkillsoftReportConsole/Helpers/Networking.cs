using System;
using System.Net;

namespace SkillsoftReportConsole.Helpers
{
    public static class Networking
    {
        /// <summary>
        /// Sets the default proxy.
        /// </summary>
        /// <param name="proxyserver">The proxyserver.</param>
        /// <param name="proxyusername">The proxyusername.</param>
        /// <param name="proxypassword">The proxypassword.</param>
        /// <param name="proxydomain">The proxydomain.</param>
        public static void SetDefaultProxy(String proxyserver, String proxyusername, String proxypassword, String proxydomain)
        {
            WebProxy _proxy = null;
            //If proxyserver command line is set then use it
            //We are not using the default proxy so we need to create a new proxy
            //Create the proxy server
            _proxy = new WebProxy(proxyserver);

            //Create the credentials if we have them set
            if (!String.IsNullOrEmpty(proxyusername))
            {
                NetworkCredential _proxyCredentials = new System.Net.NetworkCredential(proxyusername, proxypassword, proxydomain);
                _proxy.Credentials = _proxyCredentials;
            }

            //Set as the default for all webrequests
            WebRequest.DefaultWebProxy = _proxy;
        }

        /// <summary>
        /// Enables TLS 1.2 if supported
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void EnableTLS12()
        {
            Boolean platformSupportsTls12 = false;
            String platformProtocols = "";

            if (!ServicePointManager.SecurityProtocol.HasFlag((SecurityProtocolType)3072))
            {
                foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
                {
                    platformProtocols += protocol.ToString() + ",";
                    if (protocol.GetHashCode() == 3072)
                    {
                        platformSupportsTls12 = true;
                    }
                }

                if (platformSupportsTls12)
                {
                    //Enable TLS 1.2 if not already enabled.
                    ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                }
                else
                {
                    throw new Exception(String.Format("TLS 1.2 not supported. Supported Protocols: {0}", platformProtocols));
                }
            }
        }
    }
}
