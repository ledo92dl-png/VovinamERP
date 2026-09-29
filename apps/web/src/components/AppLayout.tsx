import {
  Banknote,
  BookOpen,
  ClipboardCheck,
  GraduationCap,
  House,
  ReceiptText,
  UsersRound,
} from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'

const navigation = [
  { to: '/', label: 'Trang chủ', icon: House },
  { to: '/students', label: 'Môn sinh', icon: UsersRound },
  { to: '/attendance', label: 'Điểm danh', icon: ClipboardCheck },
  { to: '/tuition', label: 'Học phí', icon: Banknote },
  { to: '/receipts', label: 'Phiếu thu', icon: ReceiptText },
  { to: '/classes', label: 'Lớp học', icon: BookOpen },
]

export default function AppLayout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">V</div>
          <div>
            <strong>Vovinam Tam Phước</strong>
            <span>Quản lý câu lạc bộ</span>
          </div>
        </div>

        <nav className="desktop-nav">
          {navigation.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              end={to === '/'}
              className={({ isActive }) =>
                `nav-item${isActive ? ' active' : ''}`
              }
            >
              <Icon size={20} />
              <span>{label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <GraduationCap size={20} />
          <div>
            <strong>Admin</strong>
            <span>Vovinam Tam Phước</span>
          </div>
        </div>
      </aside>

      <main className="main-content">
        <Outlet />
      </main>

      <nav className="mobile-nav">
        {navigation.slice(0, 5).map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              `mobile-nav-item${isActive ? ' active' : ''}`
            }
          >
            <Icon size={21} />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
