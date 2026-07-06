namespace Quiz.Shared.Responses
{
    public record PagedQuizzesResponseDto
    {
        public required ICollection<QuizResponseDto> Quizzes { get; init; }
        public required PaginationResponseDto Pagination { get; init; }
    }
}
