using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class NewPasswordData : DataTransferObjectBase
    {
        [Required]
        [MaxLength(32)]
        [MinLength(8)]
        public string Password { get; set; }
        
        [Required]
        public string Username { get; set; }

        [Required]
        [MaxLength(32)]
        [MinLength(8)]
        public string ConfirmPassword { get; set; }
    }
}