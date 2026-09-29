import { apiGet } from './api'
import type { BeltRank, PagedResult, Student } from './types'

export function getStudents(
  keyword = '',
  signal?: AbortSignal,
): Promise<PagedResult<Student>> {
  const params = new URLSearchParams({
    page: '1',
    pageSize: '100',
    sortBy: 'fullName',
    descending: 'false',
  })

  if (keyword.trim()) {
    params.set('keyword', keyword.trim())
  }

  return apiGet<PagedResult<Student>>(
    `/api/students?${params.toString()}`,
    signal,
  )
}

export function getStudent(
  studentId: string,
  signal?: AbortSignal,
): Promise<Student> {
  return apiGet<Student>(
    `/api/students/${encodeURIComponent(studentId)}`,
    signal,
  )
}

export function getBeltRanks(
  signal?: AbortSignal,
): Promise<BeltRank[]> {
  return apiGet<BeltRank[]>('/api/belt-ranks', signal)
}
