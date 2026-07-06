using System.ComponentModel.DataAnnotations;

namespace Quiz.DataAccess.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string QuestionText { get; set; } = null!;

        public List<string> Answers { get; set; } = new();

        public int IndexOfCorrectAnswer { get; set; }

        public bool IsClosed { get; set; }

    }
}
