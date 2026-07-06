export interface joinResponseDto {
    id: number,
    title: string,
    isStarted: boolean,
    participants: string[],
    author: string,
    chat: string[]
}