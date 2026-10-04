import { apiGet } from './api'

export type StudentAttendanceScore = {
  studentId: string
  fromDate: string
  toDate: string
  requiredSessionCount: number
  actualAttendedSessionCount: number
  presentCount: number
  lateCount: number
  excusedCount: number
  absentCount: number
  crossLocationAttendanceCount: number
  extraAttendanceCount: number
  attendanceRate: number | null
  score: number | null
}

export function getStudentAttendanceScore(
  studentId: string,
  tenantId: string,
  fromDate: string,
  toDate: string,
  signal?: AbortSignal,
): Promise<StudentAttendanceScore> {
  const params = new URLSearchParams({
    tenantId,
    fromDate,
    toDate,
  })

  return apiGet<StudentAttendanceScore>(
    `/api/attendance-reports/students/${encodeURIComponent(studentId)}/score?${params.toString()}`,
    signal,
  )
}
