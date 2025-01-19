using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesCFA.Models
{
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(2, ErrorMessage = "The document type must be CC, TI or RC.")]
        [RegularExpression("^(CC|TI|RC)$", ErrorMessage = "The document type must be CC, TI or RC.")]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "The document number cannot exceed 11 characters.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "The document number must be numeric.")]
        public string DocumentNumber { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "Names can only contain letters and spaces.")]
        public string Names { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The first last name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "Names can only contain letters and spaces.")]
        public string LastName1 { get; set; }

        [StringLength(30, ErrorMessage = "The second last name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "Names can only contain letters and spaces.")]
        public string LastName2 { get; set; }

        [Required]
        [StringLength(1, ErrorMessage = "Gender must be 'F' or 'M'.")]
        [RegularExpression("^(F|M)$", ErrorMessage = "Gender must be 'F' for female or 'M' for male.")]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public ICollection<Address> Addresses { get; set; }
        public ICollection<Phone> Phones { get; set; }
        public ICollection<Email> Emails { get; set; }

        public bool IsValidDocumentTypeForAge(out string errorMessage)
        {
            errorMessage = string.Empty;
            var currentDate = DateTime.Now;
            var age = currentDate.Year - BirthDate.Year;
            if (BirthDate.Date > currentDate.AddYears(-age))
                age--;

            if (age >= 0 && age <= 7 && DocumentType != "RC")
            {
                errorMessage = "For ages 0-7, only 'Registro Civil (RC)' is allowed.";
                return false;
            }

            if (age >= 8 && age <= 17 && DocumentType != "TI")
            {
                errorMessage = "For ages 8-17, only 'Tarjeta Identidad (TI)' is allowed.";
                return false;
            }

            if (age >= 18 && DocumentType != "CC")
            {
                errorMessage = "For ages 18 and above, only 'Cédula de Ciudadanía (CC)' is allowed.";
                return false;
            }

            return true;
        }
    }
}
