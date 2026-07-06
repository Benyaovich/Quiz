import type { extendedQuestionResponseDto } from '@stores/responseDtos/extendedQuestionResponseDto'

export interface extendedQuizResponseDto {
    id: number,
    pin: string,
    title: string,
    questions: extendedQuestionResponseDto[],
    participants: string[],
    isStarted: boolean,
    indexOfActiveQuestion: number,
    isPublished: boolean
}