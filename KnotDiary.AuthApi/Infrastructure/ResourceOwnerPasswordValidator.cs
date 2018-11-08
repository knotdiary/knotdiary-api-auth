using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using KnotDiary.AuthApi.Domain;
using KnotDiary.AuthApi.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Infrastructure
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public ResourceOwnerPasswordValidator(IUserService userService, ILogger logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                var username = context.UserName;
                var password = context.Password;

                var user = await _userService.GetUserByCredentials(username, password);
                if (user == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
                }
                else
                {
                    var id = user.Id;
                    context.Result = new GrantValidationResult(id, OidcConstants.AuthenticationMethods.Password, GenerateClaims(user));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
            }
        }

        private static IEnumerable<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Id", user.Id.ToString()),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("FullName", $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}"),
                new Claim("Email", user.Email ?? string.Empty)
            };

            return claims;
        }
    }
}
