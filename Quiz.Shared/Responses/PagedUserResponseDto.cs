namespace Quiz.Shared.Responses
{
    public record PagedUserResponseDto
    {
        public required UserResponseDto User { get; init; }
        public required PaginationResponseDto Pagination { get; init; }
    }
}
