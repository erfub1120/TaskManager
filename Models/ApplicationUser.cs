using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Pole 'Imię' jest obowiązkowe.")]
        [StringLength(100, ErrorMessage = "Maksymalna długość imieni 100.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Pole 'Nazwisko' jest obowiązkowe.")]
        [StringLength(100, ErrorMessage = "Maksymalna długość nazwiska 150.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Pole 'Telefon' jest obowiązkowe.")]
        [RegularExpression(@"^\+48-\d{3}-\d{3}-\d{3}$",
            ErrorMessage = "Numer telefonu musi być w formacie +48-XXX-XXX-XXX")]
        [StringLength(15)]
        public string Phone { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Nieprawidłowy format daty.")]
        [CustomValidation(typeof(ApplicationUser), nameof(ValidateDateOfBirth))]
        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Group> ManagedGroups { get; set; } = new List<Group>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();

        public static ValidationResult? ValidateDateOfBirth(DateTime? dateOfBirth, ValidationContext context)
        {
            if (dateOfBirth == null)
                return ValidationResult.Success;

            if (dateOfBirth > DateTime.Today)
                return new ValidationResult("Data urodzenia nie może być z przyszłości");

            if (dateOfBirth < DateTime.Today.AddYears(-150))
                return new ValidationResult("Nieprawidłowa data urodzenia");

            return ValidationResult.Success;
        }
    }
}
