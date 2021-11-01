using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class ForgotPasswordData : DataTransferObjectBase
    {
        [Required] [EmailAddress] public string Email { get; set; }
        [Required]  public string Name { get; set; }
    }
}