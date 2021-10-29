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
        [Required]
        public Role Role { get; set; }

        [Required] public string Name { get; set; }

        [Required]
        [MaxLength(32), MinLength(8)]
        public string Password { get; set; }
    }
}