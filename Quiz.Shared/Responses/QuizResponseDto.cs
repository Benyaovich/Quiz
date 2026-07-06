namespace Quiz.Shared.Responses
{
    public record QuizResponseDto
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required bool IsStarted { get; init; }
        public required string Author { get; init; }
    }
}
