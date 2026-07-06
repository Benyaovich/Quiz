import type { AxiosError } from 'axios'
import type { problemDetail } from '@utils/problemDetail'

export function showProblemDetail(error: AxiosError) {
    const axiosError = error as AxiosError
    if (axiosError.response) {
        const axiosErrorData = axiosError.response.data as problemDetail
        return axiosErrorData.detail
    }
    return 'An error occured'
}