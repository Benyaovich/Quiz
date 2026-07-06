import type { quizAccessRequestDto } from '@stores/requestDtos/quizAccessRequestDto'

export interface chatRequestDto{
    quizAccess: quizAccessRequestDto
    message: string
}