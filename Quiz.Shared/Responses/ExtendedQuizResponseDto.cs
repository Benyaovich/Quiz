namespace Quiz.Shared.Responses
{
    public record ExtendedQuizResponseDto
    {
        public required int Id { get; init; }
        public string? Pin { get; init; }
        public required string Title { get; init; }
        public required ICollection<ExtendedQuestionResponseDto> Questions { get; init; }
        public required List<string> Participants { get; init; }
        public required bool IsStarted { get; init; }
        public required int IndexOfActiveQuestion { get; init; }
        public required bool IsPublished { get; init; }
    }
}
