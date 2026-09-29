import {
  Banknote,
  ChevronRight,
  ClipboardCheck,
  ReceiptText,
  UsersRound,
} from 'lucide-react'
import { Link } from 'react-router-dom'

const actions = [
  {
    to: '/students',
    title: 'Môn sinh',
    description: 'Danh sách và hồ sơ môn sinh',
    icon: UsersRound,
  },
  {
    to: '/attendance',
    title: 'Điểm danh',
    description: 'Điểm danh buổi tập hôm nay',
    icon: ClipboardCheck,
  },
  {
    to: '/tuition',
    title: 'Học phí',
    description: 'Theo dõi và thu học phí',
    icon: Banknote,
  },
  {
    to: '/receipts',
    title: 'Phiếu thu',
    description: 'Phiếu thu và lịch sử thanh toán',
    icon: ReceiptText,
  },
]

export default function DashboardPage() {
  return (
    <div className="page">
      <header className="page-header hero-header">
        <div>
          <span className="eyebrow">VOVINAM TAM PHƯỚC</span>
          <h1>Xin chào!</h1>
          <p>Quản lý hoạt động câu lạc bộ hôm nay.</p>
        </div>
        <div className="avatar">VT</div>
      </header>

      <section className="stats-grid">
        <article className="stat-card">
          <span>Môn sinh đang tập</span>
          <strong>--</strong>
          <small>Sẽ lấy từ API Dashboard</small>
        </article>

        <article className="stat-card">
          <span>Điểm danh hôm nay</span>
          <strong>--</strong>
          <small>Sẽ lấy từ API Attendance</small>
        </article>

        <article className="stat-card">
          <span>Học phí cần theo dõi</span>
          <strong>--</strong>
          <small>Sẽ lấy từ API Finance</small>
        </article>
      </section>

      <section>
        <div className="section-heading">
          <div>
            <span className="eyebrow">THAO TÁC NHANH</span>
            <h2>Hôm nay bạn muốn làm gì?</h2>
          </div>
        </div>

        <div className="action-grid">
          {actions.map(({ to, title, description, icon: Icon }) => (
            <Link className="action-card" to={to} key={to}>
              <div className="action-icon">
                <Icon size={24} />
              </div>
              <div>
                <strong>{title}</strong>
                <span>{description}</span>
              </div>
              <ChevronRight size={20} />
            </Link>
          ))}
        </div>
      </section>

      <section className="notice-card">
        <div>
          <span className="eyebrow">TRẠNG THÁI</span>
          <h2>Frontend MVP đã hoạt động</h2>
          <p>
            Bước tiếp theo chúng ta sẽ nối các màn hình này với API
            VovinamERP đang có.
          </p>
        </div>
      </section>
    </div>
  )
}
