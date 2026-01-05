using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa grupy jest wymagana")]
        [StringLength(200, ErrorMessage = "Nazwa nie może być dłuższa niż 200 znaków")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Opis nie może być dłuższy niż 1000 znaków")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? ManagerId { get; set; }
        public ApplicationUser? Manager { get; set; }

        public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public bool HasActiveTasks()
        {
            return Tasks.Any(t => t.Status != TaskStatus.Done && t.Status != TaskStatus.Cancelled);
        }
        public bool CanBeDeleted()
        {
            return !HasActiveTasks();
        }
    }
}
