using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using System.Windows;
using System.Security.Cryptography.X509Certificates;


namespace ExplicitWordMonitor.ProxyFilter
{
    public class WebFilterProxy
    {
        private readonly ProxyServer proxyServer;
        private ExplicitProxyEndPoint explicitEndPoint;
        private List<string> badWords;
        private bool isRunning = false;
        private bool isCertificateInstalled;

        public WebFilterProxy(List<string> badWords)
        {
            this.badWords = badWords;
            proxyServer = new ProxyServer();

            // Check if the certificate is already installed
            isCertificateInstalled = IsCertificateInStore("Titanium Root Certificate Authority");

            if (!isCertificateInstalled)
            {
                // Trust the root certificate and install it
                try
                {
                    proxyServer.CertificateManager.CreateRootCertificate(true);
                    proxyServer.CertificateManager.TrustRootCertificate(true);
                    isCertificateInstalled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to install root certificate. HTTPS traffic will not be intercepted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Subscribe to the response event
            proxyServer.BeforeResponse += OnResponse;
        }
        private bool IsCertificateInStore(string certificateName)
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);

                foreach (var cert in store.Certificates)
                {
                    if (cert.Subject.Contains(certificateName))
                    {
                        return true; // Certificate is already installed
                    }
                }
            }
            return false; // Certificate not found
        }

        public void Start()
        {
            explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8000, true);
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            // Set as system proxy
            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            isRunning = true;
        }

        public void Stop()
        {
            if (!isRunning)
            {
                return; 
            }

            // Disable system proxy
            proxyServer.RestoreOriginalProxySettings();

            // Stop the proxy server before removing the certificate
            proxyServer.Stop();

            isRunning = false;
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Response.StatusCode == 200)
            {
                string contentType = e.HttpClient.Response.ContentType;
                if (contentType != null && contentType.Contains("text/html"))
                {
                    string body = await e.GetResponseBodyAsString();
                    if (ContainsBadWord(body))
                    {
                        // Modify response to block content
                        string blockPage = "<html><body><h1>Content Blocked</h1></body></html>";
                        e.Ok(blockPage);
                    }
                }
            }
        }

        private bool ContainsBadWord(string content)
        {
            return badWords.Any(word => content.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public void UpdateBadWords(List<string> newBadWords)
        {
            this.badWords = newBadWords;
        }
    }
}
