namespace SRSS.IAM.Services.UserService
{
    public interface ICurrentUserService
    {
        (string userId, string userRole) GetCurrentUser();
        string GetUserId();
        string GetUserName();
        string GetUserRole();
        bool IsAdmin();
    }
}
