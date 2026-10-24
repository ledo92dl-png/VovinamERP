import { useEffect, useState } from 'react'
import { AlertCircle, LoaderCircle } from 'lucide-react'
import {
  getStudentAttendanceScoreHistory,
} from '../lib/attendanceApi'
import type {
  StudentAttendanceScoreHistory,
} from '../lib/attendanceApi'

type Props = {
  studentId: string
  tenantId: string
}

function formatLocalDate(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function getHistoryRange() {
  const now = new Date()

  return {
    fromDate: formatLocalDate(
      new Date(now.getFullYear(), now.getMonth() - 5, 1),
    ),
    toDate: formatLocalDate(
      new Date(now.getFullYear(), now.getMonth() + 1, 0),
    ),
  }
}

function formatNumber(value: number) {
  return value.toLocaleString('vi-VN', {
    maximumFractionDigits: 2,
  })
}

export default function StudentAttendanceHistory({
  studentId,
  tenantId,
}: Props) {
  const [history, setHistory] =
    useState<StudentAttendanceScoreHistory | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadHistory() {
      try {
        setLoading(true)
        setError(null)
        setHistory(null)

        const { fromDate, toDate } = getHistoryRange()

        const result = await getStudentAttendanceScoreHistory(
          studentId,
          tenantId,
          fromDate,
          toDate,
          controller.signal,
        )

        if (!controller.signal.aborted) {
          setHistory(result)
        }
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError('Không thể tải lịch sử chuyên cần.')
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadHistory()

    return () => controller.abort()
  }, [studentId, tenantId])

  return (
    <section className="detail-card">
      <div className="form-section-heading">
        <h2>Lịch sử chuyên cần</h2>
        <p>Điểm chuyên cần trong 6 tháng gần nhất.</p>
      </div>

      {loading && (
        <div className="detail-row">
          <span>Đang tải lịch sử...</span>
          <LoaderCircle className="spin" size={20} />
        </div>
      )}

      {error && (
        <div className="form-error">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}

      {!loading && !error && history?.months.length === 0 && (
        <div className="detail-row">
          <span>Chưa có lịch sử chuyên cần.</span>
        </div>
      )}

      {!loading && !error &&
        [...(history?.months ?? [])]
          .reverse()
          .map((month) => (
            <div
              className="detail-row"
              key={`${month.year}-${month.month}`}
            >
              <div>
                <strong>
                  Tháng {month.month}/{month.year}
                </strong>
                <div>
                  <small>
                    Tham gia {month.actualAttendedSessionCount}
                    {' / '}
                    {month.requiredSessionCount} buổi
                  </small>
                </div>
              </div>

              <div style={{ textAlign: 'right' }}>
                <strong>
                  {month.score === null
                    ? 'Chưa có dữ liệu'
                    : `${formatNumber(month.score)} / 100`}
                </strong>

                {month.attendanceRate !== null && (
                  <div>
                    <small>
                      Tỷ lệ: {formatNumber(month.attendanceRate * 100)}%
                    </small>
                  </div>
                )}
              </div>
            </div>
          ))}
    </section>
  )
}
