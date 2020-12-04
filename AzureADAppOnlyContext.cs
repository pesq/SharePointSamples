
# Create a .Net console app using .Net 4.7.2, then add Microsoft.IdentityModel.Clients.ActiveDirectory nuget package

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace TestAzureADAppOnlyContext
{
    class Program
    {
        static void Main(string[] args)
        {
            Sample().Wait();
        }

        static Task Sample()
        {

            return Task.Run(async () => {

                string siteUrl = "https://coaparas.sharepoint.com/";
                string clientID = "1f4a4bd1-0987-4840-9e3c-8f2767a2b4b4";
                string tenant = "coaparas.onmicrosoft.com";
                string pfxCertificatePath = @"c:\work\TestAppOnlyContext.pfx";
                string certificatePass = "123456";

                // set the authentication context
                //you can do multi-tenant app-only, but you cannot use /common for authority...must get tenant ID
                string authority = $"https://login.microsoftonline.com/{tenant}/";
                AuthenticationContext authenticationContext = new AuthenticationContext(authority, false);

                //read the certificate private key from the executing location
                //NOTE: This is a hack...Azure Key Vault is best approach
                var certfile = System.IO.File.OpenRead(pfxCertificatePath);
                var certificateBytes = new byte[certfile.Length];
                certfile.Read(certificateBytes, 0, (int)certfile.Length);
                var cert = new X509Certificate2(
                    certificateBytes,
                    certificatePass,
                    X509KeyStorageFlags.Exportable |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet); //switchest are important to work in webjob
                ClientAssertionCertificate cac = new ClientAssertionCertificate(clientID, cert);

                //get the access token to SharePoint using the ClientAssertionCertificate
                Console.WriteLine("Getting app-only access token to SharePoint Online");
                var authenticationResult = await authenticationContext.AcquireTokenAsync(siteUrl, cac);

                var token = authenticationResult.AccessToken;
                Console.WriteLine($"App-only access token retreived: {token}");
                Console.ReadLine();
            });

        }
    }
}
