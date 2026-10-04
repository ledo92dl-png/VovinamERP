import {
  AlertCircle,
  ArrowLeft,
  CalendarDays,
  ChevronRight,
  ClipboardList,
  LoaderCircle,
  MapPin,
  Medal,
  UsersRound,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  getBeltExam,
  getBeltExamStudentResults,
  getBeltExamSubjects,
  type BeltExamDetail,
  type BeltExamStudentResult,
  type BeltExamSubject,
} from '../lib/beltExamsApi'
import { getAppContext } from '../lib/studentsApi'

function formatExamDate(value: string) {
  const parts = value.slice(0, 10).split('-')

  if (parts.length !== 3) return value

  const [year, month, day] = parts
  return `${day}/${month}/${year}`
}

function getResultLabel(result: number) {
  if (result === 1) return 'Đạt'
  if (result === 2) return 'Không đạt'
  return 'Chưa xác định'
}

export default function BeltExamDetailPage() {
  const { beltExamId } = useParams()

  const [exam, setExam] = useState<BeltExamDetail | null>(null)
  const [subjects, setSubjects] = useState<BeltExamSubject[]>([])
  const [results, setResults] = useState<BeltExamStudentResult[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadData() {
      if (!beltExamId) {
        setError('Không xác định được kỳ thi đai.')
        setLoading(false)
        return
      }

      try {
        setLoading(true)
        setError(null)

        const context = await getAppContext(controller.signal)

        const [examResult, subjectResult, studentResult] =
          await Promise.all([
            getBeltExam(
              beltExamId,
              context.tenantId,
              controller.signal,
            ),
            getBeltExamSubjects(
              beltExamId,
              context.tenantId,
              controller.signal,
            ),
            getBeltExamStudentResults(
              beltExamId,
              context.tenantId,
              controller.signal,
            ),
          ])

        setExam(examResult)
        setSubjects(subjectResult)
        setResults(studentResult)
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải chi tiết kỳ thi đai. Hãy kiểm tra API VovinamERP đang chạy.',
        )
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadData()

    return () => controller.abort()
  }, [beltExamId])

  if (loading) {
    return (
      <div className="page">
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải kỳ thi đai...</strong>
          <span>Đang kết nối với VovinamERP API.</span>
        </div>
      </div>
    )
  }

  if (error || !exam) {
    return (
      <div className="page">
        <Link className="back-link" to="/belt-exams">
          <ArrowLeft size={18} />
          Danh sách kỳ thi
        </Link>

        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được kỳ thi</strong>
          <span>{error ?? 'Không tìm thấy kỳ thi đai.'}</span>
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <Link className="back-link" to="/belt-exams">
        <ArrowLeft size={18} />
        Danh sách kỳ thi
      </Link>

      <header className="page-header belt-exam-detail-header">
        <div>
          <span className="eyebrow">CHI TIẾT KỲ THI ĐAI</span>
          <h1>{exam.sessionName}</h1>
          <p>
            Theo dõi nội dung thi, Môn sinh dự thi và kết quả.
          </p>
        </div>
      </header>

      <section className="belt-exam-summary">
        <article className="info-card">
          <CalendarDays size={22} />
          <span>Ngày thi</span>
          <strong>{formatExamDate(exam.examDate)}</strong>
        </article>

        <article className="info-card">
          <MapPin size={22} />
          <span>Địa điểm</span>
          <strong>{exam.location}</strong>
        </article>

        <article className="info-card">
          <Medal size={22} />
          <span>Cấp thi trên nguồn</span>
          <strong>{exam.sourceBeltName}</strong>
        </article>

        <article className="info-card">
          <UsersRound size={22} />
          <span>Môn sinh dự thi</span>
          <strong>{results.length}</strong>
        </article>
      </section>

      {exam.note && (
        <section className="notice-card belt-exam-note">
          <div>
            <span className="eyebrow">GHI CHÚ KỲ THI</span>
            <p>{exam.note}</p>
          </div>
        </section>
      )}

      <section className="belt-exam-section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">NỘI DUNG THI</span>
            <h2>{subjects.length} nội dung</h2>
          </div>
        </div>

        {subjects.length === 0 ? (
          <div className="state-card compact-state">
            <ClipboardList size={25} />
            <strong>Chưa có nội dung thi</strong>
            <span>Kỳ thi này chưa được khai báo nội dung chấm điểm.</span>
          </div>
        ) : (
          <div className="belt-exam-subjects">
            {subjects.map((subject) => (
              <div className="belt-exam-subject" key={subject.id}>
                <span>{subject.displayOrder}</span>

                <div>
                  <strong>{subject.name}</strong>
                  <small>
                    {subject.maximumScore === null
                      ? 'Không giới hạn điểm tối đa'
                      : `Điểm tối đa: ${subject.maximumScore}`}
                  </small>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="belt-exam-section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">KẾT QUẢ MÔN SINH</span>
            <h2>{results.length} Môn sinh</h2>
          </div>
        </div>

        {results.length === 0 ? (
          <div className="state-card compact-state">
            <UsersRound size={25} />
            <strong>Chưa có kết quả</strong>
            <span>
              Chưa có Môn sinh nào được ghi nhận trong kỳ thi này.
            </span>
          </div>
        ) : (
          <div className="belt-exam-results">
            {results.map((result) => (
              <Link
                key={result.id}
                className="belt-exam-result-row"
                to={`/belt-exams/${exam.id}/results/${result.id}`}
              >
                <div className="student-avatar">
                  <UsersRound size={21} />
                </div>

                <div className="belt-exam-result-main">
                  <strong>{result.fullName}</strong>

                  <span>
                    {result.unitName}
                    {result.totalScore !== null
                      ? ` · ${result.totalScore} điểm`
                      : ''}
                  </span>

                  <small>
                    {getResultLabel(result.result)}
                    {result.sourceResult
                      ? ` · Nguồn: ${result.sourceResult}`
                      : ''}
                    {result.ranking !== null
                      ? ` · Xếp hạng ${result.ranking}`
                      : ''}
                  </small>
                </div>

                <ChevronRight size={20} />
              </Link>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}