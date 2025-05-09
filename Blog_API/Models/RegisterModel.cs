using System.ComponentModel.DataAnnotations;

namespace Blog_API.Models
{
    public class RegisterModel
    {
        [Required]
        public string Name { get; set; }
                
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
