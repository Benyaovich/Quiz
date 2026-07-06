namespace Quiz.Shared.Responses
{
    public record JoinResponseDto
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required bool IsStarted { get; init; }
        public required ICollection<string> Participants { get; init; }
        public required string Author { get; init; }
        public required List<string> Chat { get; init; }
    }
}
