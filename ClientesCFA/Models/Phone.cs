using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesCFA.Models
{
    public class Phone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The phone number cannot exceed 20 characters.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "The phone must contain only numbers.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The phone type cannot exceed 20 characters.")]
        public string PhoneType { get; set; }
    }
}
