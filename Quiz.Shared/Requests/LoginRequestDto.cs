using System.ComponentModel.DataAnnotations;

namespace Quiz.Shared.Requests
{
    public record LoginRequestDto
    {
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public required string Email { get; init; }

        public required string Password { get; init; }
    }
}
