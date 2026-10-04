import {
  AlertCircle,
  ArrowLeft,
  CheckCircle2,
  ClipboardCheck,
  LoaderCircle,
  Medal,
  UserRound,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  getBeltExam,
  getBeltExamScoreSheet,
  getBeltExamStudentResults,
  getStudentBeltRecognitions,
  addBeltRankDocument,
  getBeltRankDocumentScan,
  recognizeBeltExamResult,
  uploadBeltRankDocumentScan,
  type BeltExamDetail,
  type BeltExamScoreSheet,
  type BeltExamStudentResult,
  type BeltRankRecognition,
} from '../lib/beltExamsApi'
import { getAppContext } from '../lib/studentsApi'

function getDocumentTypeLabel(documentType: number) {
  if (documentType === 1) return 'Giấy chứng nhận'
  if (documentType === 2) return 'Bằng đẳng cấp'
  return 'Văn bằng'
}

function formatDate(value: string) {
  const [year, month, day] = value.split('-')
  if (!year || !month || !day) return value
  return `${day}/${month}/${year}`
}

function getResultLabel(result: number) {
  if (result === 1) return 'Đạt'
  if (result === 2) return 'Không đạt'
  return 'Chưa xác định'
}

export default function BeltExamStudentResultPage() {
  const { beltExamId, studentResultId } = useParams()

  const [exam, setExam] = useState<BeltExamDetail | null>(null)
  const [result, setResult] = useState<BeltExamStudentResult | null>(null)
  const [scoreSheet, setScoreSheet] = useState<BeltExamScoreSheet | null>(null)
  const [tenantId, setTenantId] = useState('')
  const [recognitionDate, setRecognitionDate] = useState('')
  const [recognitionNote, setRecognitionNote] = useState('')
  const [recognizing, setRecognizing] = useState(false)
  const [recognitionError, setRecognitionError] = useState<string | null>(null)
  const [recognition, setRecognition] =
    useState<BeltRankRecognition | null>(null)
  const [documentNumber, setDocumentNumber] = useState('')
  const [documentSignedDate, setDocumentSignedDate] = useState('')
  const [documentNote, setDocumentNote] = useState('')
  const [documentSaving, setDocumentSaving] = useState(false)
  const [documentError, setDocumentError] = useState<string | null>(null)
  const [scanFile, setScanFile] = useState<File | null>(null)
  const [scanUploading, setScanUploading] = useState(false)
  const [scanUploadError, setScanUploadError] = useState<string | null>(null)
  const [scanLoading, setScanLoading] = useState(false)
  const [scanError, setScanError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadData() {
      if (!beltExamId || !studentResultId) {
        setError('Đường dẫn kết quả kỳ thi không hợp lệ.')
        setLoading(false)
        return
      }

      try {
        setLoading(true)
        setError(null)

        const context = await getAppContext(controller.signal)
        setTenantId(context.tenantId)

        const [examData, results, scoreSheetData] = await Promise.all([
          getBeltExam(
            beltExamId,
            context.tenantId,
            controller.signal,
          ),
          getBeltExamStudentResults(
            beltExamId,
            context.tenantId,
            controller.signal,
          ),
          getBeltExamScoreSheet(
            beltExamId,
            studentResultId,
            context.tenantId,
            controller.signal,
          ),
        ])

        const studentResult =
          results.find((item) => item.id === studentResultId) ?? null

        if (!studentResult) {
          setError('Không tìm thấy kết quả của Môn sinh trong kỳ thi này.')
          return
        }

        const recognitions = await getStudentBeltRecognitions(
          studentResult.studentId,
          context.tenantId,
          controller.signal,
        )

        const existingRecognition =
          recognitions.items.find(
            (item) =>
              item.beltExamStudentResultId === studentResultId,
          ) ?? null

        setExam(examData)
        setResult(studentResult)
        setScoreSheet(scoreSheetData)
        setRecognitionDate(examData.examDate)
        setRecognition(existingRecognition)
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải bảng điểm Môn sinh. Hãy kiểm tra API VovinamERP đang chạy.',
        )
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadData()

    return () => controller.abort()
  }, [beltExamId, studentResultId])

  async function handleRecognizeBelt() {
    if (
      !beltExamId ||
      !studentResultId ||
      !tenantId ||
      !recognitionDate
    ) {
      setRecognitionError('Thiếu thông tin để công nhận đai.')
      return
    }

    try {
      setRecognizing(true)
      setRecognitionError(null)

      const response = await recognizeBeltExamResult(
        beltExamId,
        studentResultId,
        {
          tenantId,
          recognitionDate,
          note: recognitionNote.trim() || null,
        },
      )

      if (!result) {
        throw new Error('Không tìm thấy thông tin Môn sinh.')
      }

      const recognitionHistory = await getStudentBeltRecognitions(
        result.studentId,
        tenantId,
      )

      const createdRecognition =
        recognitionHistory.items.find(
          (item) => item.id === response.id,
        ) ?? null

      if (!createdRecognition) {
        throw new Error(
          'Đã công nhận đai nhưng chưa tải lại được mốc công nhận.',
        )
      }

      setRecognition(createdRecognition)
    } catch (err) {
      console.error(err)
      setRecognitionError(
        err instanceof Error
          ? err.message
          : 'Không thể công nhận đai cho Môn sinh.',
      )
    } finally {
      setRecognizing(false)
    }
  }

  async function handleCreateDocument() {
    if (!recognition || !tenantId) {
      setDocumentError('Không tìm thấy thông tin công nhận đai.')
      return
    }

    if (!documentNumber.trim() || !documentSignedDate) {
      setDocumentError('Vui lòng nhập số văn bằng và ngày ký.')
      return
    }

    if (recognition.level < 2) {
      setDocumentError(
        'Cấp đai này chưa hỗ trợ tạo Bằng/Giấy chứng nhận.',
      )
      return
    }

    const documentType = recognition.level >= 6 ? 2 : 1

    try {
      setDocumentSaving(true)
      setDocumentError(null)

      const response = await addBeltRankDocument(
        recognition.id,
        {
          tenantId,
          documentType,
          documentNumber: documentNumber.trim(),
          signedDate: documentSignedDate,
          scanUrl: null,
          note: documentNote.trim() || null,
        },
      )

      setRecognition({
        ...recognition,
        document: {
          id: response.id,
          documentType,
          documentNumber: documentNumber.trim(),
          signedDate: documentSignedDate,
          scanUrl: null,
          note: documentNote.trim() || null,
        },
      })

      setDocumentNumber('')
      setDocumentSignedDate('')
      setDocumentNote('')
    } catch (err) {
      console.error(err)
      setDocumentError(
        err instanceof Error
          ? err.message
          : 'Không thể tạo Bằng/Giấy chứng nhận.',
      )
    } finally {
      setDocumentSaving(false)
    }
  }

  async function handleUploadScan() {
    if (!recognition?.document || !tenantId || !scanFile) {
      setScanUploadError('Vui lòng chọn file scan.')
      return
    }

    try {
      setScanUploading(true)
      setScanUploadError(null)

      const response = await uploadBeltRankDocumentScan(
        recognition.document.id,
        tenantId,
        scanFile,
      )

      setRecognition({
        ...recognition,
        document: {
          ...recognition.document,
          scanUrl: response.scanUrl,
        },
      })

      setScanFile(null)
    } catch (err) {
      console.error(err)
      setScanUploadError(
        err instanceof Error
          ? err.message
          : 'Không thể tải bản scan lên.',
      )
    } finally {
      setScanUploading(false)
    }
  }

  async function handleViewScan() {
    if (!recognition?.document || !tenantId) {
      setScanError('Không tìm thấy thông tin bản scan.')
      return
    }

    try {
      setScanLoading(true)
      setScanError(null)

      const blob = await getBeltRankDocumentScan(
        recognition.document.id,
        tenantId,
      )

      const blobUrl = URL.createObjectURL(blob)
      const newWindow = window.open(blobUrl, '_blank', 'noopener,noreferrer')

      if (!newWindow) {
        URL.revokeObjectURL(blobUrl)
        throw new Error(
          'Trình duyệt đã chặn cửa sổ xem bản scan.',
        )
      }

      window.setTimeout(() => {
        URL.revokeObjectURL(blobUrl)
      }, 60000)
    } catch (err) {
      console.error(err)
      setScanError(
        err instanceof Error
          ? err.message
          : 'Không thể mở bản scan.',
      )
    } finally {
      setScanLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="page">
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải bảng điểm...</strong>
          <span>Đang đọc kết quả và điểm từng nội dung thi.</span>
        </div>
      </div>
    )
  }

  if (error || !exam || !result || !scoreSheet) {
    return (
      <div className="page">
        <Link
          className="back-link"
          to={
            beltExamId
              ? `/belt-exams/${beltExamId}`
              : '/belt-exams'
          }
        >
          <ArrowLeft size={18} />
          Quay lại kỳ thi
        </Link>

        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được bảng điểm</strong>
          <span>{error ?? 'Không tìm thấy dữ liệu bảng điểm.'}</span>
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <Link
        className="back-link"
        to={`/belt-exams/${exam.id}`}
      >
        <ArrowLeft size={18} />
        Chi tiết kỳ thi
      </Link>

      <header className="page-header belt-exam-detail-header">
        <div>
          <span className="eyebrow">BẢNG ĐIỂM MÔN SINH</span>
          <h1>{result.fullName}</h1>
          <p>
            {exam.sessionName} · {result.unitName}
          </p>
        </div>
      </header>

      <section className="belt-exam-summary">
        <article className="info-card">
          <UserRound size={22} />
          <span>Kết quả</span>
          <strong>{getResultLabel(result.result)}</strong>
        </article>

        <article className="info-card">
          <Medal size={22} />
          <span>Kết quả trên nguồn</span>
          <strong>{result.sourceResult || 'Chưa có'}</strong>
        </article>

        <article className="info-card">
          <ClipboardCheck size={22} />
          <span>Đã chấm</span>
          <strong>
            {scoreSheet.scoredSubjectCount}/{scoreSheet.totalSubjectCount}
          </strong>
        </article>

        <article className="info-card">
          <CheckCircle2 size={22} />
          <span>Tổng điểm</span>
          <strong>{scoreSheet.calculatedTotalScore}</strong>
        </article>
      </section>

      {!scoreSheet.isComplete && (
        <section className="notice-card belt-exam-note">
          <AlertCircle size={22} />
          <div>
            <strong>Bảng điểm chưa hoàn tất</strong>
            <p>
              Còn nội dung thi chưa được ghi điểm cho Môn sinh này.
            </p>
          </div>
        </section>
      )}

      {scoreSheet.hasTotalScoreMismatch === true && (
        <section className="notice-card belt-exam-note">
          <AlertCircle size={22} />
          <div>
            <strong>Tổng điểm chưa khớp</strong>
            <p>
              Tổng điểm ghi nhận là{' '}
              {scoreSheet.recordedTotalScore ?? 'chưa có'}, trong khi
              tổng tính từ các nội dung là{' '}
              {scoreSheet.calculatedTotalScore}.
            </p>
          </div>
        </section>
      )}

      <section className="belt-exam-section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">CHI TIẾT ĐIỂM</span>
            <h2>{scoreSheet.totalSubjectCount} nội dung</h2>
          </div>
        </div>

        {scoreSheet.subjects.length === 0 ? (
          <div className="state-card compact-state">
            <ClipboardCheck size={25} />
            <strong>Chưa có nội dung chấm điểm</strong>
            <span>Kỳ thi chưa có nội dung điểm để hiển thị.</span>
          </div>
        ) : (
          <div className="belt-exam-score-list">
            {scoreSheet.subjects.map((subject) => (
              <article
                className="belt-exam-score-row"
                key={subject.subjectId}
              >
                <div className="belt-exam-score-order">
                  {subject.displayOrder}
                </div>

                <div className="belt-exam-score-main">
                  <strong>{subject.subjectName}</strong>
                  <span>
                    Điểm tối đa:{' '}
                    {subject.maximumScore === null
                      ? 'Không giới hạn'
                      : subject.maximumScore}
                  </span>
                </div>

                <div className="belt-exam-score-value">
                  <span>Điểm</span>
                  <strong>
                    {subject.isScored && subject.score !== null
                      ? subject.score
                      : '—'}
                  </strong>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <section className="belt-exam-section">
        <div className="belt-exam-total-card">
          <div>
            <span>Tổng điểm ghi nhận</span>
            <strong>
              {scoreSheet.recordedTotalScore ?? 'Chưa có'}
            </strong>
          </div>

          <div>
            <span>Tổng tính từ chi tiết</span>
            <strong>{scoreSheet.calculatedTotalScore}</strong>
          </div>

          <div>
            <span>Xếp hạng</span>
            <strong>{result.ranking ?? '—'}</strong>
          </div>
        </div>
      </section>

      <section className="belt-exam-section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">CÔNG NHẬN ĐAI</span>
            <h2>Xác nhận kết quả lên đai</h2>
          </div>
        </div>

        {result.result !== 1 ? (
          <div className="notice-card belt-exam-note">
            <AlertCircle size={22} />
            <div>
              <strong>Chưa thể công nhận đai</strong>
              <p>
                Chỉ Môn sinh có kết quả Đạt mới thực hiện công nhận đai.
              </p>
            </div>
          </div>
        ) : recognition ? (
          <div className="notice-card belt-exam-note">
            <CheckCircle2 size={22} />
            <div>
              <strong>Đã công nhận đai</strong>
              <p>
                Mốc công nhận đã được tạo thành công. Bước tiếp theo có
                thể cập nhật Bằng/Giấy chứng nhận và bản scan.
              </p>
            </div>
          </div>
        ) : (
          <div className="belt-recognition-form">
            <label>
              <span>Ngày công nhận</span>
              <input
                type="date"
                value={recognitionDate}
                onChange={(event) =>
                  setRecognitionDate(event.target.value)
                }
              />
            </label>

            <label>
              <span>Ghi chú</span>
              <textarea
                value={recognitionNote}
                onChange={(event) =>
                  setRecognitionNote(event.target.value)
                }
                placeholder="Ghi chú công nhận đai nếu có"
                rows={3}
              />
            </label>

            {recognitionError && (
              <div className="notice-card belt-exam-note">
                <AlertCircle size={20} />
                <div>
                  <strong>Không thể công nhận đai</strong>
                  <p>{recognitionError}</p>
                </div>
              </div>
            )}

            <button
              className="primary-button"
              type="button"
              disabled={recognizing || !recognitionDate}
              onClick={() => void handleRecognizeBelt()}
            >
              {recognizing ? (
                <>
                  <LoaderCircle className="spin" size={18} />
                  Đang công nhận...
                </>
              ) : (
                <>
                  <CheckCircle2 size={18} />
                  Công nhận đai
                </>
              )}
            </button>
          </div>
        )}
      </section>

      {recognition && !recognition.document && (
        <section className="belt-exam-section">
          <div className="section-heading">
            <div>
              <span className="eyebrow">BẰNG / GIẤY CHỨNG NHẬN</span>
              <h2>
                {recognition.level >= 6
                  ? 'Thêm Bằng đẳng cấp'
                  : 'Thêm Giấy chứng nhận'}
              </h2>
            </div>
          </div>

          <div className="belt-recognition-form belt-document-form">
            <div className="belt-document-type">
              <span>Loại văn bằng</span>
              <strong>
                {recognition.level >= 6
                  ? 'Bằng đẳng cấp'
                  : 'Giấy chứng nhận'}
              </strong>
            </div>

            <label>
              <span>Số văn bằng</span>
              <input
                type="text"
                value={documentNumber}
                onChange={(event) =>
                  setDocumentNumber(event.target.value)
                }
                placeholder="Nhập số văn bằng"
              />
            </label>

            <label>
              <span>Ngày ký</span>
              <input
                type="date"
                value={documentSignedDate}
                onChange={(event) =>
                  setDocumentSignedDate(event.target.value)
                }
              />
            </label>

            <label>
              <span>Ghi chú</span>
              <textarea
                value={documentNote}
                onChange={(event) =>
                  setDocumentNote(event.target.value)
                }
                placeholder="Ghi chú nếu có"
                rows={3}
              />
            </label>

            {documentError && (
              <div className="notice-card belt-exam-note">
                <AlertCircle size={20} />
                <div>
                  <strong>Không thể lưu văn bằng</strong>
                  <p>{documentError}</p>
                </div>
              </div>
            )}

            <button
              className="primary-button"
              type="button"
              disabled={
                documentSaving ||
                !documentNumber.trim() ||
                !documentSignedDate
              }
              onClick={() => void handleCreateDocument()}
            >
              {documentSaving
                ? 'Đang lưu...'
                : 'Lưu văn bằng'}
            </button>
          </div>
        </section>
      )}

      {recognition?.document && (
        <section className="belt-exam-section">
          <div className="section-heading">
            <div>
              <span className="eyebrow">BẰNG / GIẤY CHỨNG NHẬN</span>
              <h2>
                {getDocumentTypeLabel(recognition.document.documentType)}
              </h2>
            </div>
          </div>

          <div className="belt-rank-document-card">
            <div>
              <span>Số văn bằng</span>
              <strong>{recognition.document.documentNumber}</strong>
            </div>

            <div>
              <span>Ngày ký</span>
              <strong>{formatDate(recognition.document.signedDate)}</strong>
            </div>

            <div>
              <span>Đai được công nhận</span>
              <strong>{recognition.beltName}</strong>
            </div>

            {!recognition.document.scanUrl && (
              <div className="belt-rank-document-scan">
                <span>Bản scan</span>

                <input
                  type="file"
                  accept=".pdf,image/jpeg,image/png,image/webp"
                  onChange={(event) => {
                    setScanFile(event.target.files?.[0] ?? null)
                    setScanUploadError(null)
                  }}
                />

                {scanFile && (
                  <strong>{scanFile.name}</strong>
                )}

                <button
                  className="secondary-button"
                  type="button"
                  disabled={scanUploading || !scanFile}
                  onClick={() => void handleUploadScan()}
                >
                  {scanUploading
                    ? 'Đang tải lên...'
                    : 'Tải bản scan lên'}
                </button>

                {scanUploadError && (
                  <p className="form-error">{scanUploadError}</p>
                )}
              </div>
            )}

            {recognition.document.scanUrl && (
              <div className="belt-rank-document-scan">
                <span>Bản scan</span>
                <button
                  className="secondary-button"
                  type="button"
                  disabled={scanLoading}
                  onClick={() => void handleViewScan()}
                >
                  {scanLoading ? 'Đang mở...' : 'Xem bản scan'}
                </button>

                {scanError && (
                  <p className="form-error">{scanError}</p>
                )}
              </div>
            )}

            {recognition.document.note && (
              <div className="belt-rank-document-note">
                <span>Ghi chú</span>
                <p>{recognition.document.note}</p>
              </div>
            )}
          </div>
        </section>
      )}

      {result.note && (
        <section className="notice-card belt-exam-note">
          <div>
            <span className="eyebrow">GHI CHÚ KẾT QUẢ</span>
            <p>{result.note}</p>
          </div>
        </section>
      )}
    </div>
  )
}