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

export type StudentStatusFilter =
  | 1
  | 2
  | 3
  | 5
  | 'all'

export async function getStudents(
  keyword = '',
  signal?: AbortSignal,
  status: StudentStatusFilter = 2,
): Promise<PagedResult<Student>> {
  const pageSize = 100
  let page = 1
  let totalCount = 0
  let totalPages = 1
  const allStudents: Student[] = []

  do {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
      sortBy: 'fullName',
      descending: 'false',
    })

    if (status !== 'all') {
      params.set('status', String(status))
    }

    if (keyword.trim()) {
      params.set('keyword', keyword.trim())
    }

    const result = await apiGet<PagedResult<Student>>(
      `/api/students?${params.toString()}`,
      signal,
    )

    allStudents.push(...result.items)

    totalCount = result.totalCount
    totalPages = result.totalPages
    page += 1
  } while (page <= totalPages)

  return {
    items: allStudents,
    page: 1,
    pageSize: allStudents.length,
    totalCount,
    totalPages,
    hasPreviousPage: false,
    hasNextPage: false,
  }
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
export type UpdateStudentInput = {
  tenantId: string
  fullName: string
  gender: number
  dateOfBirth: string | null
  phoneNumber: string | null
  email: string | null
  address: string | null
  avatarUrl: string | null
  currentBeltRankId: string | null
  martialName: string | null
  introducedBy: string | null
  martialProfileNote: string | null
}

export async function updateStudent(
  studentId: string,
  input: UpdateStudentInput,
  signal?: AbortSignal,
): Promise<Student> {
  const response = await fetch(`${API_BASE_URL}/api/students/${studentId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(input),
    signal,
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      message || `Unable to update student (${response.status}).`,
    )
  }

  return response.json() as Promise<Student>
}
export type ChangeStudentStatusInput = {
  tenantId: string
  status: number
  reason: string | null
}

export async function changeStudentStatus(
  studentId: string,
  input: ChangeStudentStatusInput,
): Promise<Student> {
  const response = await fetch(
    `${API_BASE_URL}/api/students/${encodeURIComponent(studentId)}/status`,
    {
      method: 'PUT',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    },
  )

  if (!response.ok) {
    const message = await response.text()

    throw new Error(
      message || `Không thể thay đổi trạng thái Môn sinh (${response.status}).`,
    )
  }

  return response.json() as Promise<Student>
}