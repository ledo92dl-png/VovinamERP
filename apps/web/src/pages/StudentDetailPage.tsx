import {
  AlertCircle,
  ArrowLeft,
  Banknote,
  CalendarDays,
  ClipboardCheck,
  LoaderCircle,
  QrCode,
  UserRound,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  changeStudentStatus,
  getBeltRanks,
  getStudent,
} from '../lib/studentsApi'
import type { Student } from '../lib/types'

const STUDENT_STATUS = {
  Trial: 1,
  Active: 2,
  Paused: 3,
  Left: 5,
} as const

function getStatusName(status: number | string) {
  switch (Number(status)) {
    case STUDENT_STATUS.Trial:
      return 'Học thử'
    case STUDENT_STATUS.Active:
      return 'Đang theo tập'
    case STUDENT_STATUS.Paused:
      return 'Tạm nghỉ'
    case STUDENT_STATUS.Left:
      return 'Đã nghỉ'
    default:
      return 'Không xác định'
  }
}

function formatDate(value: string | null) {
  if (!value) return 'Chưa cập nhật'

  const [year, month, day] = value.slice(0, 10).split('-')

  if (!year || !month || !day) return value

  return `${day}/${month}/${year}`
}

export default function StudentDetailPage() {
  const { studentId } = useParams()

  const [student, setStudent] = useState<Student | null>(null)
  const [beltName, setBeltName] = useState('Chưa xếp đai')
  const [loading, setLoading] = useState(true)
  const [changingStatus, setChangingStatus] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!studentId) {
      setError('Không tìm thấy mã Môn sinh.')
      setLoading(false)
      return
    }

    const controller = new AbortController()

    async function loadStudent() {
      try {
        setLoading(true)
        setError(null)

        const [studentResult, beltRanks] = await Promise.all([
          getStudent(studentId!, controller.signal),
          getBeltRanks(controller.signal),
        ])

        setStudent(studentResult)

        const belt = beltRanks.find(
          (item) => item.id === studentResult.currentBeltRankId,
        )

        setBeltName(belt?.beltName ?? 'Chưa xếp đai')
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải hồ sơ Môn sinh. Hãy kiểm tra API VovinamERP.',
        )
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadStudent()

    return () => controller.abort()
  }, [studentId])

  async function handleStatusChange(status: number) {
    if (!student || !studentId) return

    let reason: string | null = null

    if (status !== STUDENT_STATUS.Active) {
      const enteredReason = window.prompt(
        `Lý do chuyển sang "${getStatusName(status)}":`,
      )

      if (enteredReason === null) return

      reason = enteredReason.trim() || null
    }

    try {
      setChangingStatus(true)
      setError(null)

      const updatedStudent = await changeStudentStatus(studentId, {
        tenantId: student.tenantId,
        status,
        reason,
      })

      setStudent(updatedStudent)
    } catch (err) {
      console.error(err)

      setError(
        err instanceof Error
          ? err.message
          : 'Không thể thay đổi trạng thái Môn sinh.',
      )
    } finally {
      setChangingStatus(false)
    }
  }

  return (
    <div className="page">
      <Link className="back-link" to="/students">
        <ArrowLeft size={18} />
        Danh sách Môn sinh
      </Link>

      {loading && (
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải hồ sơ...</strong>
          <span>Đang lấy dữ liệu Môn sinh từ API.</span>
        </div>
      )}

      {!loading && error && !student && (
        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được hồ sơ</strong>
          <span>{error}</span>
        </div>
      )}

      {!loading && student && (
        <>
          <section className="profile-card">
            <div className="profile-avatar">
              <UserRound size={34} />
            </div>

            <div className="profile-title">
              <span className="eyebrow">HỒ SƠ MÔN SINH</span>
              <h1>{student.fullName}</h1>
              <p>
                {student.memberNumber} · {beltName}
              </p>
            </div>

            <Link
              className="secondary-button"
              to={`/students/${student.studentId}/edit`}
            >
              Chỉnh sửa
            </Link>
          </section>

          {error && (
            <div className="form-error">
              <AlertCircle size={18} />
              <span>{error}</span>
            </div>
          )}

          <section className="detail-card">
            <div className="detail-row">
              <span>Trạng thái</span>
              <strong>{getStatusName(student.status)}</strong>
            </div>

            <div className="detail-row">
              <span>Ngày sinh</span>
              <strong>{formatDate(student.dateOfBirth)}</strong>
            </div>

            <div className="detail-row">
              <span>Ngày tham gia CLB</span>
              <strong>{formatDate(student.enrollmentDate)}</strong>
            </div>

            <div className="detail-row">
              <span>Số điện thoại</span>
              <strong>{student.phoneNumber || 'Chưa cập nhật'}</strong>
            </div>

            <div className="detail-row">
              <span>Email</span>
              <strong>{student.email || 'Chưa cập nhật'}</strong>
            </div>

            <div className="detail-row">
              <span>Địa chỉ</span>
              <strong>{student.address || 'Chưa cập nhật'}</strong>
            </div>
          </section>

          <section className="form-section">
            <div className="form-section-heading">
              <h2>Trạng thái tập luyện</h2>
              <p>
                Thay đổi trạng thái nhưng vẫn giữ nguyên hồ sơ và lịch sử
                của Môn sinh.
              </p>
            </div>

            <div className="form-actions">
              {Number(student.status) !== STUDENT_STATUS.Active && (
                <button
                  className="primary-button"
                  type="button"
                  disabled={changingStatus}
                  onClick={() =>
                    void handleStatusChange(STUDENT_STATUS.Active)
                  }
                >
                  Quay lại tập
                </button>
              )}

              {Number(student.status) === STUDENT_STATUS.Active && (
                <>
                  <button
                    className="secondary-button"
                    type="button"
                    disabled={changingStatus}
                    onClick={() =>
                      void handleStatusChange(STUDENT_STATUS.Paused)
                    }
                  >
                    Tạm nghỉ
                  </button>

                  <button
                    className="secondary-button"
                    type="button"
                    disabled={changingStatus}
                    onClick={() =>
                      void handleStatusChange(STUDENT_STATUS.Left)
                    }
                  >
                    Đã nghỉ
                  </button>
                </>
              )}
            </div>
          </section>

          <div className="profile-grid">
            <article className="info-card">
              <QrCode size={23} />
              <div>
                <span>Mã QR</span>
                <strong>QR vĩnh viễn</strong>
              </div>
            </article>

            <article className="info-card">
              <ClipboardCheck size={23} />
              <div>
                <span>Điểm danh</span>
                <strong>Lịch sử tham gia</strong>
              </div>
            </article>

            <article className="info-card">
              <Banknote size={23} />
              <div>
                <span>Học phí</span>
                <strong>Lịch sử đóng phí</strong>
              </div>
            </article>

            <article className="info-card">
              <CalendarDays size={23} />
              <div>
                <span>Lớp đang học</span>
                <strong>Nhiều lớp</strong>
              </div>
            </article>
          </div>
        </>
      )}
    </div>
  )
}