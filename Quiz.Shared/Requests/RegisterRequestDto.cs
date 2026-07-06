using System.ComponentModel.DataAnnotations;

namespace Quiz.Shared.Requests
{
    public record RegisterRequestDto
    {
        [MinLength(3,ErrorMessage = "Username is too short")]
        public required string UserName { get; init; }

        [EmailAddress(ErrorMessage = "Email is invalid")]
        public required string Email { get; init; }

        public required string Password { get; init; }

    }
}
