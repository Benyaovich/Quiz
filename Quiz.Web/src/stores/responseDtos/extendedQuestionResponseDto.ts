export interface extendedQuestionResponseDto {
    id: number,
    questionText: string,
    answers: string[],
    indexOfCorrectAnswer: number,
    isClosed: boolean
}