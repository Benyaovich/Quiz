namespace Quiz.Shared.Responses
{
    public record ExtendedQuestionResponseDto
    {
        public required int Id { get; init; }
        public required string QuestionText { get; init; }
        public required List<string> Answers { get; init; }
        public required int IndexOfCorrectAnswer { get; init; }
        public required bool IsClosed { get; init; }
    }
}
