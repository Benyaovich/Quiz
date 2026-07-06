namespace Quiz.Shared.Requests
{
    public record JoinRequestDto
    {
        public required QuizAccessRequestDto QuizAccess { get; init; }
    }
}
