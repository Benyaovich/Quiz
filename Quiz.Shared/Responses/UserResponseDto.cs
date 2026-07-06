namespace Quiz.Shared.Responses
{
    public record UserResponseDto
    {
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required ICollection<ExtendedQuizResponseDto> Quizzes { get; init; }

    }
}
