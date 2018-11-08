using Newtonsoft.Json;
using KnotDiary.AuthApi.Domain;
using KnotDiary.Services.Http;
using Serilog;
using System.Threading.Tasks;
using KnotDiary.Models;

namespace KnotDiary.AuthApi.Http
{
    public class HttpUserService : IHttpUserService
    {
        private readonly ILogger _logger;
        private readonly HttpClientWrapper _httpClient;
        private readonly HttpUserServiceSettings _settings;

        public HttpUserService(ILogger logger, HttpUserServiceSettings httpSettings, JsonSerializerSettings serializerSettings)
        {
            _logger = logger;
            _settings = httpSettings;
            _httpClient = new HttpClientWrapper(_logger, httpSettings.BaseUrl, httpSettings.TimeoutInMilliseconds, serializerSettings);
        }

        public async Task<BaseResponse<User>> GetByCredentials(string username, string password)
        {
            _logger.Information($"HttpUserService | GetByCredentials | Calling UsersApi | username: {username}");
            var result = await _httpClient.GetAsync<BaseResponse<User>>($"api/user/credentials/?username={username}&password={password}");
            return result;
        }

        public async Task<BaseResponse<User>> GetUserById(string id)
        {
            _logger.Information($"HttpUserService | GetUser | Calling UsersApi | id: {id}");
            var result = await _httpClient.GetAsync<BaseResponse<User>>($"api/user/id/{id}");
            return result;
        }
    }
}
