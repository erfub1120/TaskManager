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
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Data akcji jest wymagana")]
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Akcja jest wymagana")]
        public LogAction Action { get; set; }

        [StringLength(2000, ErrorMessage = "Opis nie może być dłuższy niż 2000 znaków")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Log musi być powiązany z zadaniem")]
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskTitle { get; set; } = string.Empty;

        [Required]
        public TaskStatus TaskStatus { get; set; }

        [Required]
        public TaskPriority TaskPriority { get; set; }

        [Required]
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        [StringLength(200)]
        public string GroupName { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }

        [StringLength(100)]
        public string? AssignedUserFirstName { get; set; }
        [StringLength(200)]
        public string? AssignedUserLastName { get; set; }
    }
}
