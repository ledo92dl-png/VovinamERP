import { ChevronRight, Search, UserRound } from 'lucide-react'
import { Link } from 'react-router-dom'

type Student = {
  id: string
  memberNumber: string
  fullName: string
  birthYear: number
  belt: string
}

const sampleStudents: Student[] = [
  {
    id: 'ms-001',
    memberNumber: 'MS001',
    fullName: 'Nguyễn Minh Anh',
    birthYear: 2012,
    belt: 'Lam đai',
  },
  {
    id: 'ms-002',
    memberNumber: 'MS002',
    fullName: 'Trần Gia Huy',
    birthYear: 2011,
    belt: 'Lam đai',
  },
  {
    id: 'ms-003',
    memberNumber: 'MS003',
    fullName: 'Lê Hoàng Nam',
    birthYear: 2010,
    belt: 'Hoàng đai',
  },
  {
    id: 'ms-004',
    memberNumber: 'MS004',
    fullName: 'Phạm Khánh Linh',
    birthYear: 2013,
    belt: 'Tự vệ nhập môn',
  },
]

const beltOrder = ['Tự vệ nhập môn', 'Lam đai', 'Hoàng đai']

export default function StudentsPage() {
  const grouped = beltOrder
    .map((belt) => ({
      belt,
      students: sampleStudents.filter((student) => student.belt === belt),
    }))
    .filter((group) => group.students.length > 0)

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">QUẢN LÝ MÔN SINH</span>
          <h1>Môn sinh đang theo tập</h1>
          <p>Danh sách được nhóm theo đai hiện tại.</p>
        </div>
        <button className="primary-button" type="button">
          + Thêm môn sinh
        </button>
      </header>

      <div className="search-box">
        <Search size={20} />
        <input
          type="search"
          placeholder="Tìm theo tên hoặc mã môn sinh..."
          aria-label="Tìm môn sinh"
        />
      </div>

      <div className="belt-groups">
        {grouped.map((group) => (
          <section className="belt-group" key={group.belt}>
            <div className="belt-heading">
              <div>
                <span className="belt-dot" />
                <h2>{group.belt}</h2>
              </div>
              <span>{group.students.length} môn sinh</span>
            </div>

            <div className="student-list">
              {group.students.map((student) => (
                <Link
                  to={`/students/${student.id}`}
                  className="student-row"
                  key={student.id}
                >
                  <div className="student-avatar">
                    <UserRound size={22} />
                  </div>

                  <div className="student-main">
                    <strong>{student.fullName}</strong>
                    <span>
                      {student.memberNumber} · Sinh năm {student.birthYear}
                    </span>
                  </div>

                  <ChevronRight size={20} />
                </Link>
              ))}
            </div>
          </section>
        ))}
      </div>

      <p className="prototype-note">
        Dữ liệu đang là dữ liệu mẫu để kiểm tra giao diện. Bước tiếp theo
        sẽ lấy danh sách thật từ API.
      </p>
    </div>
  )
}
