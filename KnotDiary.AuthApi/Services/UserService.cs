using KnotDiary.AuthApi.Domain;
using KnotDiary.AuthApi.Http;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpUserService _httpUserService;

        public UserService(IHttpUserService httpUserService)
        {
            _httpUserService = httpUserService;
        }

        public async Task<User> GetUserById(string id)
        {
            var result = await _httpUserService.GetUserById(id);
            if (result?.Data != null && result.IsSuccess)
            {
                return result.Data;
            }

            return null;
        }

        public async Task<User> GetUserByCredentials(string username, string password)
        {
            var result = await _httpUserService.GetByCredentials(username, password);
            if (result?.Data != null && result.IsSuccess)
            {
                return result.Data;
            }

            return null;
        }
    }
}
