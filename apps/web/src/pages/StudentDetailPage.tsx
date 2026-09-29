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
import { getBeltRanks, getStudent } from '../lib/studentsApi'
import type { Student } from '../lib/types'

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

  return (
    <div className="page">
      <Link className="back-link" to="/students">
        <ArrowLeft size={18} />
        Danh sách môn sinh
      </Link>

      {loading && (
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải hồ sơ...</strong>
          <span>Đang lấy dữ liệu Môn sinh từ API.</span>
        </div>
      )}

      {!loading && error && (
        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được hồ sơ</strong>
          <span>{error}</span>
        </div>
      )}

      {!loading && !error && student && (
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

            <button className="secondary-button" type="button">
              Chỉnh sửa
            </button>
          </section>

          <section className="detail-card">
            <div className="detail-row">
              <span>Ngày sinh</span>
              <strong>{formatDate(student.dateOfBirth)}</strong>
            </div>

            <div className="detail-row">
              <span>Ngày nhập môn</span>
              <strong>{formatDate(student.enrollmentDate)}</strong>
            </div>

            <div className="detail-row">
              <span>Võ danh</span>
              <strong>{student.martialName || 'Chưa cập nhật'}</strong>
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
