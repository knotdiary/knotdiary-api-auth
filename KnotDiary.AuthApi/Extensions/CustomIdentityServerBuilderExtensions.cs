using Microsoft.Extensions.DependencyInjection;
using KnotDiary.AuthApi.Infrastructure;

namespace KnotDiary.AuthApi.Extensions
{
    public static class CustomIdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddResourceOwnerPasswordValidator(this IIdentityServerBuilder builder)
        {
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            return builder;
        }

        public static IIdentityServerBuilder AddUserProfileService(this IIdentityServerBuilder builder)
        {
            builder.AddProfileService<UserProfileService>();
            return builder;
        }
    }
}
