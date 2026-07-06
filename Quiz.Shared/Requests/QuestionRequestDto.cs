using System.ComponentModel.DataAnnotations;

namespace Quiz.Shared.Requests
{
    public record QuestionRequestDto
    {
        [MinLength(3,ErrorMessage = "Question is too short")]
        [MaxLength(255,ErrorMessage = "Question is too long")]
        public required string QuestionText { get; init; }

        [MinLength(2,ErrorMessage = "Minimum number of answers is 2")]
        [MaxLength(4,ErrorMessage = "Maximum number of answers is 4")]
        public required ICollection<string> Answers { get; init; }

        [Range(0,4,ErrorMessage = "Invalid index")]
        public required int IndexOfCorrectAnswer { get; init; }

    }
}
