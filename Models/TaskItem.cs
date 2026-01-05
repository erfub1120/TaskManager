using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        InReview,
        Done,
        Cancelled
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł zadania jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł musi do 200 znaków")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Opis nie może być dłuższy niż 2000 znaków")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status jest wymagany")]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        [Required(ErrorMessage = "Priorytet jest wymagany")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(TaskItem), nameof(ValidateDueDate))]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Zadanie musi być przypisane do grupy")]
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        [Required(ErrorMessage = "Twórca zadania jest wymagany")]
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }

        public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();

        public static ValidationResult? ValidateDueDate(DateTime? dueDate, ValidationContext context)
        {
            if (dueDate == null)
                return ValidationResult.Success;

            if (dueDate < DateTime.Today)
                return new ValidationResult("Data zakończenia nie może być w przeszłości");

            if (dueDate > DateTime.Today.AddYears(1))
                return new ValidationResult("Data zakończenia jest zbyt odległa (maksymalnie 1 rok)");

            return ValidationResult.Success;
        }
    }
}
