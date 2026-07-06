import type { quizResponseDto } from '@stores/responseDtos/quizResponseDto'
import type { paginationResponseDto } from '@stores/responseDtos/paginationResponseDto'

export interface pagedQuizzesResponseDto{
    quizzes: quizResponseDto[],
    pagination: paginationResponseDto
}