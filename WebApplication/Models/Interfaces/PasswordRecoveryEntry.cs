namespace WebApplication.Models.Interfaces
{
    public class PasswordRecoveryEntry
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string DateCreate { get; set; }
        public string ExpirationDate { get; set; }
    }
}