using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class UserLoginData : DataTransferObjectBase
    {
        [Required(ErrorMessage = "Username is empty!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is empty!")]
        public string Password { get; set; }
    }
}