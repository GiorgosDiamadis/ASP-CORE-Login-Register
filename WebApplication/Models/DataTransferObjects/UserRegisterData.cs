using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class UserRegisterDto : DataTransferObjectBase
    {
        [Required] public Role Role { get; set; }

        [Required] public string Name { get; set; }

        [Required] [Phone] public string PhoneNumber { get; set; }

        [Required] [EmailAddress] public string Email { get; set; }

        [Required]
        [MaxLength(32)]
        [MinLength(8)]
        public string Password { get; set; }
        
        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}