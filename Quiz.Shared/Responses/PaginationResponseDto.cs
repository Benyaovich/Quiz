namespace Quiz.Shared.Responses
{
    public record PaginationResponseDto
    {
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
    }
}
