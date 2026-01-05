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

        public string? ManagerId { get; set; }
        public ApplicationUser? Manager { get; set; }

        public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
