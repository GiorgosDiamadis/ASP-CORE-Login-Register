using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.DataTransferObjects
{
    public class ForgotPasswordData : DataTransferObjectBase
    {
        [Required]  public string Name { get; set; }
    }
}