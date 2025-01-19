using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesCFA.Models
{
    public class Email
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The email cannot exceed 255 characters.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        public string EmailAddress { get; set; }
    }
}
