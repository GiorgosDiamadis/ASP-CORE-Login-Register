using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Models.DataTransferObjects
{
    public class UserRegisterData : DataTransferObjectBase
    {
        [Required] public Role Role { get; set; }

        [Required(ErrorMessage = "You must provide a username!")]
        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        [RegularExpression("^[A-Za-z]*$", ErrorMessage = "Special characters and spaces are not allowed!")]
        public string Name { get; set; }


        [Required(ErrorMessage = "You must provide a phone number!")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Not a valid phone number!")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "You must provide an email address!")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must create a password!")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",
            ErrorMessage =
                "Password should be at least 8 and at most 32 characters long, have at least one upper, one lower case, one special character and one number!")]
     
        public string Password { get; set; }

        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}