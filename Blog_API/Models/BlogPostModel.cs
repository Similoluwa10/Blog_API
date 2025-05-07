using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        //[Required]
        //[MaxLength(20)]
        //public string Author { get; set; }

        private int UserId { get; set; }
        private UserModel User { get; set; }
    }
}
