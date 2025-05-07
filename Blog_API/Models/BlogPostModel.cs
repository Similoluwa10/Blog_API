using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Blog_API.Models
{
    public class BlogPostModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(20)]        
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        // Foreign key to User - must be public
        public int UserId { get; set; }

        // Navigation property to User - must be public
        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
