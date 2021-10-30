using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class UserLoginData : DataTransferObjectBase
    {
        [Required] public string Name { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(32)]
        public string Password { get; set; }
    }
}