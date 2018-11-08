using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace KnotDiary.AuthApi.Infrastructure
{
    public class Clients
    {
        private readonly IConfiguration _configuration;

        public Clients(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public IEnumerable<Client> Get()
        {
            var allowedCorsOrigins = new List<string>();
            var configSection= _configuration.GetSection("AllowedOrigins");
            var originsConfig = _configuration.AsEnumerable();

            if (originsConfig != null)
            {
                allowedCorsOrigins.AddRange(originsConfig.Where(a => a.Value != null).Select(a => a.Value));
            }

            return new List<Client>
            {
                new Client
                {
                    ClientId = "KnotDiary_client",
                    ClientName = "KnotDiary.Auth.Client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = allowedCorsOrigins,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "custom.profile",
                        "api1", "api2.read_only"
                    }
                }
            };
        }
    }
}