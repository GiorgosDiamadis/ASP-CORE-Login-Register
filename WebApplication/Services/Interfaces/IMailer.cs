namespace WebApplication.Services.Interfaces
{
    public interface IMailer
    {
        void ForgotPassWordEmail(string email,string token);
        void ConfirmEmail(string email,string token);
    }
}