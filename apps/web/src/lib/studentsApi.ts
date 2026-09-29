import { apiGet } from './api'
import type { BeltRank, PagedResult, Student } from './types'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export type AppContext = {
  tenantId: string
  tenantCode: string
  tenantName: string
  organizationId: string
  organizationCode: string
  organizationName: string
}

export type CreateStudentInput = {
  tenantId: string
  organizationId: string
  fullName: string
  gender: number
  dateOfBirth: string | null
  phoneNumber: string | null
  email: string | null
  address: string | null
  avatarUrl: string | null
  currentBeltRankId: string | null
  enrollmentDate: string
  martialName: string | null
  introducedBy: string | null
  martialProfileNote: string | null
}

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

export function getAppContext(
  signal?: AbortSignal,
): Promise<AppContext> {
  return apiGet<AppContext>('/api/app-context', signal)
}

export async function createStudent(
  input: CreateStudentInput,
): Promise<Student> {
  const response = await fetch(`${API_BASE_URL}/api/students`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(input),
  })

  if (!response.ok) {
    const message = await response.text()

    throw new Error(
      message || `Không thể tạo Môn sinh (${response.status}).`,
    )
  }

  return response.json() as Promise<Student>
}