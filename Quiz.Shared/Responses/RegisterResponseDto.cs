namespace Quiz.Shared.Responses
{
    public record RegisterResponseDto
    {
        public required string Id { get; init; }
        public required string UserName { get; init; }
        public required string Email { get; init; }

    }
}
