using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using KnotDiary.AuthApi.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Infrastructure
{
    public class UserProfileService : IProfileService
    {
        private readonly IUserService _userService;

        public UserProfileService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = await _userService.GetUserById(subjectId);

            if (user == null)
            {
                return;
            }
            
            var userId = user.Id;

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, userId),
                new Claim(JwtClaimTypes.Name, user.Username)
            };

            if (!string.IsNullOrEmpty(user.FirstName))
            {
                claims.Add(new Claim(JwtClaimTypes.GivenName, user.FirstName));
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                claims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = await _userService.GetUserById(subjectId);

            context.IsActive = user != null;
        }
    }
}
