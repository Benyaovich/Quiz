using System.ComponentModel.DataAnnotations;

namespace Quiz.Shared.Requests
{
    public record QuizRequestDto
    {
        [MinLength(3,ErrorMessage = "Title is too short")]
        [MaxLength(255,ErrorMessage = "Title is too long")]
        public required string Title { get; init; }

        [MinLength(1)]
        [MaxLength(20)]
        public required ICollection<QuestionRequestDto> Questions { get; init; }
    }
}
