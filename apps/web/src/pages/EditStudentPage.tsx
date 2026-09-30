import {
  AlertCircle,
  ArrowLeft,
  LoaderCircle,
  Save,
  UserRound,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  getBeltRanks,
  getStudent,
  updateStudent,
} from '../lib/studentsApi'
import type { BeltRank, Student } from '../lib/types'

export default function EditStudentPage() {
  const { studentId } = useParams()
  const navigate = useNavigate()

  const [student, setStudent] = useState<Student | null>(null)
  const [beltRanks, setBeltRanks] = useState<BeltRank[]>([])

  const [fullName, setFullName] = useState('')
  const [gender, setGender] = useState(0)
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [email, setEmail] = useState('')
  const [address, setAddress] = useState('')
  const [introducedBy, setIntroducedBy] = useState('')
  const [martialProfileNote, setMartialProfileNote] = useState('')

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!studentId) {
      setError('Không tìm thấy mã Môn sinh.')
      setLoading(false)
      return
    }

    const controller = new AbortController()

    async function loadData() {
      try {
        setLoading(true)
        setError(null)

        const [studentResult, beltResult] = await Promise.all([
          getStudent(studentId!, controller.signal),
          getBeltRanks(controller.signal),
        ])

        if (controller.signal.aborted) return

        const activeBelts = beltResult
          .filter((belt) => belt.isActive)
          .sort(
            (a, b) =>
              a.level - b.level ||
              a.beltName.localeCompare(b.beltName, 'vi'),
          )

        setStudent(studentResult)
        setBeltRanks(activeBelts)

        setFullName(studentResult.fullName)
        setGender(Number(studentResult.gender))
        setDateOfBirth(studentResult.dateOfBirth?.slice(0, 10) ?? '')
        setPhoneNumber(studentResult.phoneNumber ?? '')
        setEmail(studentResult.email ?? '')
        setAddress(studentResult.address ?? '')
        setIntroducedBy(studentResult.introducedBy ?? '')
        setMartialProfileNote(studentResult.martialProfileNote ?? '')
      } catch (err) {
        if (controller.signal.aborted) return

        console.error(err)
        setError(
          'Không thể tải dữ liệu Môn sinh. Hãy kiểm tra API VovinamERP.',
        )
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadData()

    return () => controller.abort()
  }, [studentId])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!student || !studentId) return

    const normalizedName = fullName.trim()

    if (!normalizedName) {
      setError('Vui lòng nhập họ và tên Môn sinh.')
      return
    }

    try {
      setSaving(true)
      setError(null)

      await updateStudent(studentId, {
        tenantId: student.tenantId,
        fullName: normalizedName,
        gender,
        dateOfBirth: dateOfBirth || null,
        phoneNumber: phoneNumber.trim() || null,
        email: email.trim() || null,
        address: address.trim() || null,
        avatarUrl: null,
        introducedBy: introducedBy.trim() || null,
        martialProfileNote: martialProfileNote.trim() || null,
      })

      navigate(`/students/${studentId}`)
    } catch (err) {
      console.error(err)

      setError(
        err instanceof Error
          ? err.message
          : 'Không thể cập nhật hồ sơ Môn sinh.',
      )
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="page">
        <div className="state-card">
          <LoaderCircle className="spin" size={28} />
          <strong>Đang tải hồ sơ...</strong>
          <span>Đang chuẩn bị dữ liệu chỉnh sửa.</span>
        </div>
      </div>
    )
  }

  if (error && !student) {
    return (
      <div className="page">
        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không tải được hồ sơ</strong>
          <span>{error}</span>
        </div>
      </div>
    )
  }

  if (!student) return null

  return (
    <div className="page">
      <Link className="back-link" to={`/students/${student.studentId}`}>
        <ArrowLeft size={18} />
        Quay lại hồ sơ
      </Link>

      <section className="profile-card">
        <div className="profile-avatar">
          <UserRound size={34} />
        </div>

        <div className="profile-title">
          <span className="eyebrow">CHỈNH SỬA MÔN SINH</span>
          <h1>{student.fullName}</h1>
          <p>{student.memberNumber}</p>
        </div>
      </section>

      <form className="student-form" onSubmit={handleSubmit}>
        {error && (
          <div className="form-error">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <section className="form-section">
          <div className="form-section-heading">
            <h2>Thông tin cá nhân</h2>
            <p>Cập nhật thông tin cơ bản của Môn sinh.</p>
          </div>

          <div className="form-grid">
            <label className="form-field form-field-wide">
              <span>Họ và tên *</span>
              <input
                type="text"
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                required
              />
            </label>

            <label className="form-field">
              <span>Giới tính</span>
              <select
                value={gender}
                onChange={(event) => setGender(Number(event.target.value))}
              >
                <option value={0}>Chưa cập nhật</option>
                <option value={1}>Nam</option>
                <option value={2}>Nữ</option>
              </select>
            </label>

            <label className="form-field">
              <span>Ngày sinh</span>
              <input
                type="date"
                value={dateOfBirth}
                onChange={(event) => setDateOfBirth(event.target.value)}
              />
            </label>

            <label className="form-field">
              <span>Ngày tham gia CLB</span>
              <input
                type="date"
                value={student.enrollmentDate.slice(0, 10)}
                disabled
              />
              <small>Ngày tham gia CLB không chỉnh sửa tại màn hình này.</small>
            </label>

            <label className="form-field">
              <span>Đai hiện tại</span>
              <input
                type="text"
                value={
                  beltRanks.find(
                    (belt) => belt.id === student.currentBeltRankId,
                  )?.beltName ?? 'Chưa xếp đai'
                }
                disabled
              />
            </label>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section-heading">
            <h2>Thông tin liên hệ</h2>
          </div>

          <div className="form-grid">
            <label className="form-field">
              <span>Số điện thoại</span>
              <input
                type="tel"
                value={phoneNumber}
                onChange={(event) => setPhoneNumber(event.target.value)}
              />
            </label>

            <label className="form-field">
              <span>Email</span>
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
              />
            </label>

            <label className="form-field form-field-wide">
              <span>Địa chỉ</span>
              <input
                type="text"
                value={address}
                onChange={(event) => setAddress(event.target.value)}
              />
            </label>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section-heading">
            <h2>Thông tin CLB</h2>
          </div>

          <div className="form-grid">
            <label className="form-field form-field-wide">
              <span>Người giới thiệu</span>
              <input
                type="text"
                value={introducedBy}
                onChange={(event) => setIntroducedBy(event.target.value)}
              />
            </label>

            <label className="form-field form-field-wide">
              <span>Ghi chú</span>
              <textarea
                rows={4}
                value={martialProfileNote}
                onChange={(event) =>
                  setMartialProfileNote(event.target.value)
                }
              />
            </label>
          </div>
        </section>

        <div className="form-actions">
          <Link
            className="secondary-button"
            to={`/students/${student.studentId}`}
          >
            Hủy
          </Link>

          <button
            className="primary-button"
            type="submit"
            disabled={saving}
          >
            {saving ? (
              <>
                <LoaderCircle className="spin" size={18} />
                Đang lưu...
              </>
            ) : (
              <>
                <Save size={18} />
                Lưu thay đổi
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  )
}