namespace KnotDiary.AuthApi.Domain
{
    public enum UserErrors
    {
        UnhandledError = 0,
        UserNotFound = 1,
        UsernameInUse = 2,
        PasswordTooWeak = 3,
        EmailInUse = 4,
        FailedToUploadAvatar = 5
    }
}
