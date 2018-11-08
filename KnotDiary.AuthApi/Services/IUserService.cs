using KnotDiary.AuthApi.Domain;
using System.Threading.Tasks;

namespace KnotDiary.AuthApi.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(string id);

        Task<User> GetUserByCredentials(string username, string password);
    }
}
