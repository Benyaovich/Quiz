import type { extendedQuizResponseDto } from '@stores/responseDtos/extendedQuizResponseDto'

export interface userResponseDto {
    userName: string,
    email: string,
    quizzes: extendedQuizResponseDto[]
}