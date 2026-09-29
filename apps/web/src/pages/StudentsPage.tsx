import {
  AlertCircle,
  ChevronRight,
  LoaderCircle,
  Search,
  UserRound,
} from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getBeltRanks, getStudents } from '../lib/studentsApi'
import type { BeltRank, Student } from '../lib/types'

function getBirthYear(dateOfBirth: string | null) {
  if (!dateOfBirth) return null

  const year = Number(dateOfBirth.slice(0, 4))
  return Number.isFinite(year) ? year : null
}

export default function StudentsPage() {
  const [students, setStudents] = useState<Student[]>([])
  const [beltRanks, setBeltRanks] = useState<BeltRank[]>([])
  const [keyword, setKeyword] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [totalCount, setTotalCount] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    async function loadData() {
      try {
        setLoading(true)
        setError(null)

        const [studentResult, beltResult] = await Promise.all([
          getStudents('', controller.signal),
          getBeltRanks(controller.signal),
        ])

        setStudents(studentResult.items)
        setTotalCount(studentResult.totalCount)
        setBeltRanks(beltResult)
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải dữ liệu Môn sinh. Hãy kiểm tra API VovinamERP đang chạy.',
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

  const beltMap = useMemo(
    () => new Map(beltRanks.map((belt) => [belt.id, belt])),
    [beltRanks],
  )

  const filteredStudents = useMemo(() => {
    const normalized = keyword.trim().toLocaleLowerCase('vi')

    if (!normalized) return students

    return students.filter((student) => {
  const fullName = student.fullName.toLocaleLowerCase('vi')
  const memberNumber = student.memberNumber.toLocaleLowerCase('vi')

  return (
    fullName.includes(normalized) ||
    memberNumber.includes(normalized)
  )
})
  }, [students, keyword])

  const groupedStudents = useMemo(() => {
    const groups = new Map<
      string,
      {
        id: string
        name: string
        level: number
        students: Student[]
      }
    >()

    for (const student of filteredStudents) {
      const belt = student.currentBeltRankId
        ? beltMap.get(student.currentBeltRankId)
        : undefined

      const key = belt?.id ?? 'no-belt'

      if (!groups.has(key)) {
        groups.set(key, {
          id: key,
          name: belt?.beltName ?? 'Chưa xếp đai',
          level: belt?.level ?? Number.MAX_SAFE_INTEGER,
          students: [],
        })
      }

      groups.get(key)!.students.push(student)
    }

    return [...groups.values()].sort(
      (a, b) =>
        a.level - b.level ||
        a.name.localeCompare(b.name, 'vi'),
    )
  }, [beltMap, filteredStudents])

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">QUẢN LÝ MÔN SINH</span>
          <h1>Môn sinh đang theo tập</h1>
          <p>
            {loading
              ? 'Đang tải dữ liệu...'
              : `${totalCount} môn sinh trong hệ thống`}
          </p>
        </div>

        <Link className="primary-button" to="/students/new">
  + Thêm môn sinh
</Link>
      </header>

      <div className="search-box">
        <Search size={20} />
        <input
          type="search"
          value={keyword}
          onChange={(event) => setKeyword(event.target.value)}
          placeholder="Tìm theo tên hoặc mã môn sinh"
          aria-label="Tìm môn sinh"
        />
      </div>

      {loading && (
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải Môn sinh...</strong>
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

      {!loading && !error && groupedStudents.length === 0 && (
        <div className="state-card">
          <UserRound size={28} />
          <strong>Không tìm thấy Môn sinh</strong>
          <span>
            {keyword
              ? 'Thử tìm bằng tên hoặc mã Môn sinh khác.'
              : 'Hệ thống chưa có dữ liệu Môn sinh.'}
          </span>
        </div>
      )}

      {!loading && !error && groupedStudents.length > 0 && (
        <div className="belt-groups">
          {groupedStudents.map((group) => (
            <section className="belt-group" key={group.id}>
              <div className="belt-heading">
                <div>
                  <span className="belt-dot" />
                  <h2>{group.name}</h2>
                </div>
                <span>{group.students.length} môn sinh</span>
              </div>

              <div className="student-list">
                {group.students.map((student) => {
                  const birthYear = getBirthYear(student.dateOfBirth)

                  return (
                    <Link
                      to={`/students/${student.studentId}`}
                      className="student-row"
                      key={student.studentId}
                    >
                      <div className="student-avatar">
                        <UserRound size={22} />
                      </div>

                      <div className="student-main">
                        <strong>{student.fullName}</strong>
                        <span>
                          {student.memberNumber}
                          {birthYear ? ` · Sinh năm ${birthYear}` : ''}
                          </span>
                      </div>

                      <ChevronRight size={20} />
                    </Link>
                  )
                })}
              </div>
            </section>
          ))}
        </div>
      )}
    </div>
  )
}
