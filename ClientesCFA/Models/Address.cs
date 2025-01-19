using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesCFA.Models
{
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The address cannot exceed 255 characters.")]
        public string AddressLine { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The address type cannot exceed 30 characters.")]
        public string AddressType { get; set; }
    }
}
