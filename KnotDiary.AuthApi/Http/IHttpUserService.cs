using KnotDiary.AuthApi.Domain;
using KnotDiary.Models;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Http
{
    public interface IHttpUserService
    {
        Task<BaseResponse<User>> GetUserById(string id);

        Task<BaseResponse<User>> GetByCredentials(string username, string password);
    }
}
