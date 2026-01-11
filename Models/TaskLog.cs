using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public enum LogAction
    {
        Created,
        Updated,
        Deleted,
        StatusChanged,
        PriorityChanged,
        AssignedToUser,
        UnassignedFromUser,
        DueDateChanged
    }

    public class TaskLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Użytkownik wykonujący akcję jest wymagany")]
        [Display(Name = "Wykonawca zmiany ID")]
        public string UserId { get; set; } = string.Empty;
        [Display(Name = "Wykonawca zmiany")]
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Data akcji jest wymagana")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Data akcji")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Akcja jest wymagana")]
        [Display(Name = "Akcja")]
        public LogAction Action { get; set; }

        [StringLength(2000, ErrorMessage = "Opis nie może być dłuższy niż 2000 znaków")]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Log musi być powiązany z zadaniem")]
        [Display(Name = "Zadanie")]
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Nazwa zadania")]
        public string TaskTitle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public TaskStatus TaskStatus { get; set; }

        [Required]
        [Display(Name = "Priorytet")]
        public TaskPriority TaskPriority { get; set; }

        [Required]
        [Display(Name = "Grupa")]
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        [StringLength(200)]
        [Display(Name = "Nazwa grupy")]
        public string GroupName { get; set; } = string.Empty;

        [Display(Name = "Użytkownik")]
        public string? AssignedUserId { get; set; }

        [StringLength(100)]
        public string? AssignedUserFirstName { get; set; }
        [StringLength(200)]
        public string? AssignedUserLastName { get; set; }
    }
}
