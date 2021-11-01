using System.ComponentModel.DataAnnotations;
using WebApplication.Models.Interfaces;

namespace WebApplication.Models
{
    public enum Role
    {
        Developer = 0,
        Manager = 1
    }

    public class User : IDatabaseModel
    {
        public string Id { get; set; }
        [Required] public Role Role { get; set; }

        [Required] public string Name { get; set; }

        [Required] [Phone] public string PhoneNumber { get; set; }

        [Required] [EmailAddress] public string Email { get; set; }

        public string Hash { get; set; }
        public string Salt { get; set; }
        public int HasValidated { get; set; }
        public string ConfirmationToken { get; set; }
    }
}