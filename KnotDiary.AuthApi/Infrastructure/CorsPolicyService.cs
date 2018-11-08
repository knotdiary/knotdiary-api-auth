using IdentityServer4.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Infrastructure
{
    public class CorsPolicyService : DefaultCorsPolicyService, ICorsPolicyService
    {
        private readonly Serilog.ILogger _logger;
        private IConfiguration _configuration { get; set; }
        
        public CorsPolicyService(IConfiguration configuration, Serilog.ILogger logger, ILogger<CorsPolicyService> mslogger) : base(mslogger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        async Task<bool> ICorsPolicyService.IsOriginAllowedAsync(string origin)
        {
            if (AllowAll)
            {
                _logger.Error("AllowAll true, so origin: {0} is allowed", origin);
                return await Task.FromResult(true);
            }
            
            var allowedCorsOrigins = new List<string>();
            var configSection = _configuration.GetSection("AllowedOrigins");
            var originsConfig = _configuration.AsEnumerable();

            if (originsConfig != null)
            {
                allowedCorsOrigins.AddRange(originsConfig.Where(a => a.Value != null).Select(a => a.Value));
            }

            AllowedOrigins = allowedCorsOrigins;

            if (AllowedOrigins != null)
            {
                if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.Error("AllowedOrigins configured and origin {0} is allowed", origin);
                    return true;
                }

                _logger.Error("AllowedOrigins configured and origin {0} is not allowed", origin);
            }
            _logger.Error("Exiting; origin {0} is not allowed", origin);

            return await Task.FromResult(false);
        }
    }
}
