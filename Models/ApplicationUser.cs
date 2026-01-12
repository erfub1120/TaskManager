using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, ErrorMessage = "Maksymalna długość imieni 100.")]
        [Display(Name = "Imię")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Maksymalna długość nazwiska 150.")]
        [Display(Name = "Nazwisko")]
        public string? LastName { get; set; }

        [RegularExpression(@"^\+48-\d{3}-\d{3}-\d{3}$",
            ErrorMessage = "Numer telefonu musi być w formacie +48-XXX-XXX-XXX")]
        [StringLength(15)]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Nieprawidłowy format daty.")]
        [CustomValidation(typeof(ApplicationUser), nameof(ValidateDateOfBirth))]
        [Display(Name = "Data urodzenia")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Konto utworzono")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Group> ManagedGroups { get; set; } = new List<Group>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();

        public string GetFullName()
        {
            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                return Email ?? "Nie wiadomy użytkownik";

            return $"{FirstName} {LastName}".Trim();
        }

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
