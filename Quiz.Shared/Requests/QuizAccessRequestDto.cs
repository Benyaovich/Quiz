using System.ComponentModel.DataAnnotations;

namespace Quiz.Shared.Requests
{
    public record QuizAccessRequestDto
    {
        [MinLength(3, ErrorMessage = "Username is too short")]
        public required string UserName { get; init; }

        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Pin must be 6 digit number")]
        public required string Pin { get; init; }
    }
}
