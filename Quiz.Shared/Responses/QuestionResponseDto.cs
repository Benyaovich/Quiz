namespace Quiz.Shared.Responses
{
    public record QuestionResponseDto
    {
        public required int Id { get; init; }

        public required string QuestionText { get; init; }

        public required IList<string> Answers { get; init; }
        public required bool IsClosed { get; init; }
    }
}
