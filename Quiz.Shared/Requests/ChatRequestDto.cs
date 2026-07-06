namespace Quiz.Shared.Requests
{
    public record ChatRequestDto
    {
        public required QuizAccessRequestDto QuizAccess { get; init; }
        public required string Message { get; init; }
    }
}
