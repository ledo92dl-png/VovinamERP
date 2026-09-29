import {
  ArrowLeft,
  Banknote,
  CalendarDays,
  ClipboardCheck,
  QrCode,
  UserRound,
} from 'lucide-react'
import { Link, useParams } from 'react-router-dom'

export default function StudentDetailPage() {
  const { studentId } = useParams()

  return (
    <div className="page">
      <Link className="back-link" to="/students">
        <ArrowLeft size={18} />
        Danh sách môn sinh
      </Link>

      <section className="profile-card">
        <div className="profile-avatar">
          <UserRound size={34} />
        </div>

        <div className="profile-title">
          <span className="eyebrow">HỒ SƠ MÔN SINH</span>
          <h1>Hồ sơ môn sinh</h1>
          <p>Mã dữ liệu: {studentId}</p>
        </div>

        <button className="secondary-button" type="button">
          Chỉnh sửa
        </button>
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

      <section className="notice-card">
        <div>
          <span className="eyebrow">ĐANG HOÀN THIỆN</span>
          <h2>Hồ sơ chi tiết</h2>
          <p>
            Màn hình này sẽ hiển thị thông tin cá nhân, đai hiện tại,
            lịch sử đai, lớp đăng ký, số buổi tham gia, học phí,
            Student Credit và lịch sử thanh toán.
          </p>
        </div>
      </section>
    </div>
  )
}
