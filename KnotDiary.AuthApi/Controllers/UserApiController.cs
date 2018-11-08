using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnotDiary.AuthApi.Domain;
using KnotDiary.AuthApi.Services;
using KnotDiary.Common.Web.Controllers;
using Serilog;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KnotDiary.Common;

namespace KnotDiary.AuthApi.Controllers
{
    [Route("api/user")]
    public class UserApiController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserApiController(ILogHelper logger, IUserService userService, IConfigurationHelper configurationHelper) : base(logger, configurationHelper)
        {
            _userService = userService;
        }

        [Route("")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var user = await GetUserFromToken();
            if (user == null)
            {
                return Unauthorized();
            }

            return GetSuccessResponse(true, "Success", user);
        }
        
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<User> GetUserFromToken()
        {
            var idClaim = User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier);
            if (idClaim == null || string.IsNullOrEmpty(idClaim.Value))
            {
                _logHelper.LogError($"Failed to get user from token / No claims found");
                return null;
            }

            var user = await _userService.GetUserById(idClaim.Value);
            if (user == null)
            {
                _logHelper.LogError($"No user found for {idClaim.Value}");
                return null;
            }

            return user;
        }
    }
}
