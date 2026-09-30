export type Student = {
  studentId: string
  personId: string
  tenantId: string
  organizationId: string
  memberNumber: string
  fullName: string
  gender: number | string
  dateOfBirth: string | null
  phoneNumber: string | null
  email: string | null
  address: string | null
  currentBeltRankId: string | null
  enrollmentDate: string
  status: number | string
  introducedBy: string | null
  martialProfileNote: string | null
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type BeltRank = {
  id: string
  beltCode: string
  beltName: string
  level: number
  description: string | null
  isActive: boolean
}
