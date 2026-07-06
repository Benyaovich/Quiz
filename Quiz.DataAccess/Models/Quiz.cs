using System.ComponentModel.DataAnnotations;

namespace Quiz.DataAccess.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        public int? Pin { get; set; }

        [MaxLength(255)]
        public string Title { get; set; } = null!;

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        public List<string> Participants { get; set; } = new();

        public bool IsStarted { get; set; }

        public int IndexOfActiveQuestion { get; set; }

        public bool IsPublished { get; set; } = false;

        public User User { get; set; } = null!;

        public List<string> Chat { get; set; } = new List<string>();
    }
}
