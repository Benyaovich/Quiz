export interface questionResponseDto {
    id: number,
    questionText: string,
    answers: string[],
    isClosed: boolean
}