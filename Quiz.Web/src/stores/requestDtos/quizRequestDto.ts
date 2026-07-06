import type { questionRequestDto } from '@stores/requestDtos/questionRequestDto'

export interface quizRequestDto {
    title: string,
    questions: questionRequestDto[]
}