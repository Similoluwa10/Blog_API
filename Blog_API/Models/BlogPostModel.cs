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

        [JsonIgnore]
        public int UserId { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
