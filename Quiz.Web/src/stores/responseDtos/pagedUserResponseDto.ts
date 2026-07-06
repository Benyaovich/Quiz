import type { paginationResponseDto } from '@stores/responseDtos/paginationResponseDto'
import type { userResponseDto } from '@stores/responseDtos/userResponseDto'

export interface pagedUserResponseDto{
    user: userResponseDto,
    pagination: paginationResponseDto
}