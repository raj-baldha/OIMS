namespace OIMS.Application.Interfaces.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(
            int userId,
            string email,
            string role);
    }
}