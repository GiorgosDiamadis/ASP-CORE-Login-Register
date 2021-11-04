using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class NewPasswordData : DataTransferObjectBase
    {
        [Required(ErrorMessage = "You must create a password!")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",
            ErrorMessage =
                "Password should be at least 8 and at most 32 characters long, have at least one upper, one lower case, one special character and one number!")]
        public string Password { get; set; }

        [Required] public string Username { get; set; }

        [Required] public string Key { get; set; }

        [Required(ErrorMessage = "You must create a password!")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",
            ErrorMessage =
                "Password should be at least 8 and at most 32 characters long, have at least one upper, one lower case, one special character and one number!")]
        public string ConfirmPassword { get; set; }
    }
}