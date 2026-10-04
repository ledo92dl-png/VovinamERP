import { API_BASE_URL, ApiError, apiGet, apiPost, apiPostForm } from './api'

export type BeltExamListItem = {
  id: string
  examDate: string
  sessionName: string
  location: string
  targetBeltRankId: string
  targetBeltCode: string
  targetBeltName: string
  sourceBeltName: string
  studentCount: number
}

export type BeltExamDetail = {
  id: string
  tenantId: string
  targetBeltRankId: string
  examDate: string
  sessionName: string
  location: string
  sourceBeltName: string
  note: string | null
}

export type BeltExamStudentResult = {
  id: string
  studentId: string
  fullName: string
  unitName: string
  sourceResult: string
  result: number
  totalScore: number | null
  ranking: number | null
  note: string | null
}

export type BeltExamSubject = {
  id: string
  name: string
  displayOrder: number
  maximumScore: number | null
}

export function getBeltExams(
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltExamListItem[]> {
  const params = new URLSearchParams({
    tenantId,
  })

  return apiGet<BeltExamListItem[]>(
    `/api/belt-exams?${params.toString()}`,
    signal,
  )
}

export function getBeltExam(
  beltExamId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltExamDetail> {
  const params = new URLSearchParams({
    tenantId,
  })

  return apiGet<BeltExamDetail>(
    `/api/belt-exams/${encodeURIComponent(beltExamId)}?${params.toString()}`,
    signal,
  )
}

export function getBeltExamStudentResults(
  beltExamId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltExamStudentResult[]> {
  const params = new URLSearchParams({
    tenantId,
  })

  return apiGet<BeltExamStudentResult[]>(
    `/api/belt-exams/${encodeURIComponent(beltExamId)}/results?${params.toString()}`,
    signal,
  )
}

export function getBeltExamSubjects(
  beltExamId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltExamSubject[]> {
  const params = new URLSearchParams({
    tenantId,
  })

  return apiGet<BeltExamSubject[]>(
    `/api/belt-exams/${encodeURIComponent(beltExamId)}/subjects?${params.toString()}`,
    signal,
  )
}

export type BeltExamScoreSheetSubject = {
  subjectId: string
  subjectName: string
  displayOrder: number
  maximumScore: number | null
  scoreId: string | null
  score: number | null
  isScored: boolean
}

export type BeltExamScoreSheet = {
  beltExamStudentResultId: string
  recordedTotalScore: number | null
  calculatedTotalScore: number
  scoredSubjectCount: number
  totalSubjectCount: number
  isComplete: boolean
  hasTotalScoreMismatch: boolean | null
  subjects: BeltExamScoreSheetSubject[]
}

export function getBeltExamScoreSheet(
  beltExamId: string,
  studentResultId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltExamScoreSheet> {
  const params = new URLSearchParams({ tenantId })

  return apiGet<BeltExamScoreSheet>(
    `/api/belt-exams/${encodeURIComponent(beltExamId)}/results/${encodeURIComponent(studentResultId)}/scores?${params.toString()}`,
    signal,
  )
}
export type RecognizeBeltExamResultRequest = {
  tenantId: string
  recognitionDate: string
  note: string | null
}

export type CreateBeltRankRecognitionResponse = {
  id: string
}

export function recognizeBeltExamResult(
  beltExamId: string,
  studentResultId: string,
  request: RecognizeBeltExamResultRequest,
  signal?: AbortSignal,
): Promise<CreateBeltRankRecognitionResponse> {
  return apiPost<
    CreateBeltRankRecognitionResponse,
    RecognizeBeltExamResultRequest
  >(
    `/api/belt-exams/${encodeURIComponent(beltExamId)}/results/${encodeURIComponent(studentResultId)}/recognition`,
    request,
    signal,
  )
}

export type BeltRankDocumentType = 1 | 2

export type AddBeltRankDocumentRequest = {
  tenantId: string
  documentType: BeltRankDocumentType
  documentNumber: string
  signedDate: string
  scanUrl: string | null
  note: string | null
}

export type AddBeltRankDocumentResponse = {
  id: string
}

export function addBeltRankDocument(
  recognitionId: string,
  request: AddBeltRankDocumentRequest,
  signal?: AbortSignal,
): Promise<AddBeltRankDocumentResponse> {
  return apiPost<
    AddBeltRankDocumentResponse,
    AddBeltRankDocumentRequest
  >(
    `/api/belt-rank-recognitions/${encodeURIComponent(recognitionId)}/document`,
    request,
    signal,
  )
}

export type UploadBeltRankDocumentScanResponse = {
  scanUrl: string
}

export function uploadBeltRankDocumentScan(
  documentId: string,
  tenantId: string,
  file: File,
  signal?: AbortSignal,
): Promise<UploadBeltRankDocumentScanResponse> {
  const formData = new FormData()

  formData.append('TenantId', tenantId)
  formData.append('File', file)

  return apiPostForm<UploadBeltRankDocumentScanResponse>(
    `/api/belt-rank-documents/${encodeURIComponent(documentId)}/scan`,
    formData,
    signal,
  )
}
export type BeltRankDocument = {
  id: string
  documentType: number
  documentNumber: string
  signedDate: string
  scanUrl: string | null
  note: string | null
}

export type BeltRankRecognition = {
  id: string
  beltRankId: string
  beltCode: string
  beltName: string
  level: number
  recognitionDate: string
  source: number
  beltExamStudentResultId: string | null
  note: string | null
  document: BeltRankDocument | null
}

export type BeltRankRecognitionHistory = {
  studentId: string
  items: BeltRankRecognition[]
}

export function getStudentBeltRecognitions(
  studentId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<BeltRankRecognitionHistory> {
  const params = new URLSearchParams({ tenantId })

  return apiGet<BeltRankRecognitionHistory>(
    `/api/students/${encodeURIComponent(studentId)}/belt-recognitions?${params.toString()}`,
    signal,
  )
}
export async function getBeltRankDocumentScan(
  documentId: string,
  tenantId: string,
  signal?: AbortSignal,
): Promise<Blob> {
  const params = new URLSearchParams({ tenantId })

  const response = await fetch(
    `${API_BASE_URL}/api/belt-rank-documents/${encodeURIComponent(documentId)}/scan?${params.toString()}`,
    { signal },
  )

  if (!response.ok) {
    throw new ApiError(
      response.status,
      `Không thể tải bản scan. API trả về ${response.status}.`,
    )
  }

  return response.blob()
}
export type CreateManualBeltRecognitionRequest = {
  tenantId: string
  beltRankId: string
  recognitionDate: string
  note: string | null
  userId: string | null
}

export type CreateManualBeltRecognitionResponse = {
  beltRankRecognitionId: string
}

export function createManualBeltRecognition(
  studentId: string,
  request: CreateManualBeltRecognitionRequest,
  signal?: AbortSignal,
): Promise<CreateManualBeltRecognitionResponse> {
  return apiPost<
    CreateManualBeltRecognitionResponse,
    CreateManualBeltRecognitionRequest
  >(
    `/api/students/${encodeURIComponent(studentId)}/belt-recognitions`,
    request,
    signal,
  )
}
