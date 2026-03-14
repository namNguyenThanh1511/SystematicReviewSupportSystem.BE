namespace SRSS.IAM.Services.UserService
{
    public interface ICurrentUserService
    {
        (string userId, string userRole) GetCurrentUser();
        string GetUserId();
        string GetUserRole();
        bool IsAdmin();
    }
}
