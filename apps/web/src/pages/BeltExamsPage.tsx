import {
  AlertCircle,
  CalendarDays,
  ChevronRight,
  LoaderCircle,
  MapPin,
  Medal,
  UsersRound,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getBeltExams,
  type BeltExamListItem,
} from '../lib/beltExamsApi'
import { getAppContext } from '../lib/studentsApi'

function formatExamDate(value: string) {
  const parts = value.slice(0, 10).split('-')

  if (parts.length !== 3) return value

  const [year, month, day] = parts
  return `${day}/${month}/${year}`
}

export default function BeltExamsPage() {
  const [exams, setExams] = useState<BeltExamListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadData() {
      try {
        setLoading(true)
        setError(null)

        const context = await getAppContext(controller.signal)
        const result = await getBeltExams(
          context.tenantId,
          controller.signal,
        )

        setExams(result)
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải danh sách kỳ thi đai. Hãy kiểm tra API VovinamERP đang chạy.',
        )
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadData()

    return () => controller.abort()
  }, [])

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">QUẢN LÝ KỲ THI ĐAI</span>
          <h1>Kỳ thi đai</h1>
          <p>
            {loading
              ? 'Đang tải dữ liệu...'
              : `${exams.length} kỳ thi`}
          </p>
        </div>
      </header>

      {loading && (
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải kỳ thi đai...</strong>
          <span>Đang kết nối với VovinamERP API.</span>
        </div>
      )}

      {!loading && error && (
        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được dữ liệu</strong>
          <span>{error}</span>
        </div>
      )}

      {!loading && !error && exams.length === 0 && (
        <div className="state-card">
          <Medal size={28} />
          <strong>Chưa có kỳ thi đai</strong>
          <span>
            Các kỳ thi đai được tạo trong hệ thống sẽ xuất hiện tại đây.
          </span>
        </div>
      )}

      {!loading && !error && exams.length > 0 && (
        <div className="belt-exam-list">
          {exams.map((exam) => (
            <Link
              key={exam.id}
              to={`/belt-exams/${exam.id}`}
              className="belt-exam-card"
            >
              <div className="belt-exam-date">
                <CalendarDays size={21} />
                <span>{formatExamDate(exam.examDate)}</span>
              </div>

              <div className="belt-exam-main">
                <strong>{exam.sessionName}</strong>

                <span className="belt-exam-rank">
                  <Medal size={17} />
                  {exam.targetBeltName}
                </span>

                <span>
                  <MapPin size={17} />
                  {exam.location}
                </span>

                <span>
                  <UsersRound size={17} />
                  {exam.studentCount} Môn sinh
                </span>

                {exam.sourceBeltName !== exam.targetBeltName && (
                  <small>
                    Tên trên nguồn: {exam.sourceBeltName}
                  </small>
                )}
              </div>

              <ChevronRight
                className="belt-exam-chevron"
                size={21}
              />
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}