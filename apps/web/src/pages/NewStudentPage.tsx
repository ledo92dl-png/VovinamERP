import {
  AlertCircle,
  ArrowLeft,
  LoaderCircle,
  Save,
} from 'lucide-react'
import {
  type FormEvent,
  useEffect,
  useMemo,
  useState,
} from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  createStudent,
  getAppContext,
  getBeltRanks,
  type AppContext,
} from '../lib/studentsApi'
import type { BeltRank } from '../lib/types'

function today() {
  return new Date().toLocaleDateString('en-CA')
}

function emptyToNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

export default function NewStudentPage() {
  const navigate = useNavigate()

  const [appContext, setAppContext] =
    useState<AppContext | null>(null)
  const [beltRanks, setBeltRanks] = useState<BeltRank[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const [fullName, setFullName] = useState('')
  const [gender, setGender] = useState('0')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [enrollmentDate, setEnrollmentDate] = useState(today())
  const [currentBeltRankId, setCurrentBeltRankId] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [email, setEmail] = useState('')
  const [address, setAddress] = useState('')
  const [introducedBy, setIntroducedBy] = useState('')
  const [martialProfileNote, setMartialProfileNote] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadData() {
      try {
        setLoading(true)
        setLoadError(null)

        const [context, belts] = await Promise.all([
          getAppContext(controller.signal),
          getBeltRanks(controller.signal),
        ])

        setAppContext(context)
        setBeltRanks(belts)
      } catch (error) {
        if (controller.signal.aborted) return

        console.error(error)
        setLoadError(
          'Không thể tải thông tin CLB hoặc danh mục đai.',
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

  const sortedBelts = useMemo(
    () =>
      [...beltRanks]
        .filter((belt) => belt.isActive)
        .sort(
          (a, b) =>
            a.level - b.level ||
            a.beltName.localeCompare(b.beltName, 'vi'),
        ),
    [beltRanks],
  )

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!appContext || saving) return

    if (!fullName.trim()) {
      setSubmitError('Vui lòng nhập họ và tên Môn sinh.')
      return
    }

    if (!enrollmentDate) {
      setSubmitError('Vui lòng chọn ngày tham gia CLB.')
      return
    }

    try {
      setSaving(true)
      setSubmitError(null)

      const student = await createStudent({
        tenantId: appContext.tenantId,
        organizationId: appContext.organizationId,
        fullName: fullName.trim(),
        gender: Number(gender),
        dateOfBirth: dateOfBirth || null,
        phoneNumber: emptyToNull(phoneNumber),
        email: emptyToNull(email),
        address: emptyToNull(address),
        avatarUrl: null,
        currentBeltRankId: currentBeltRankId || null,
        enrollmentDate,
        introducedBy: emptyToNull(introducedBy),
        martialProfileNote: emptyToNull(martialProfileNote),
      })

      navigate(`/students/${student.studentId}`)
    } catch (error) {
      console.error(error)
      setSubmitError(
        error instanceof Error
          ? error.message
          : 'Không thể tạo Môn sinh.',
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
          <strong>Đang chuẩn bị hồ sơ...</strong>
          <span>Đang tải thông tin CLB và danh mục đai.</span>
        </div>
      </div>
    )
  }

  if (loadError || !appContext) {
    return (
      <div className="page">
        <div className="state-card error-state">
          <AlertCircle size={28} />
          <strong>Không thể mở form</strong>
          <span>{loadError ?? 'Không tìm thấy thông tin CLB.'}</span>
          <Link className="secondary-button" to="/students">
            Quay lại Môn sinh
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">HỒ SƠ MÔN SINH</span>
          <h1>Thêm Môn sinh</h1>
          <p>
            {appContext.organizationName} · Mã Môn sinh được tạo tự động
          </p>
        </div>

        <Link className="secondary-button" to="/students">
          <ArrowLeft size={18} />
          Quay lại
        </Link>
      </header>

      <form className="student-form" onSubmit={handleSubmit}>
        <section className="form-section">
          <div className="form-section-heading">
            <h2>Thông tin cơ bản</h2>
            <p>Các thông tin nhận diện và liên hệ của Môn sinh.</p>
          </div>

          <div className="form-grid">
            <label className="form-field form-field-wide">
              <span>Họ và tên *</span>
              <input
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                placeholder="Ví dụ: Nguyễn Văn Bình"
                autoFocus
                required
              />
            </label>

            <label className="form-field">
              <span>Giới tính *</span>
              <select
                value={gender}
                onChange={(event) => setGender(event.target.value)}
              >
                <option value="0">Nam</option>
                <option value="1">Nữ</option>
                <option value="2">Khác</option>
              </select>
            </label>

            <label className="form-field">
              <span>Ngày sinh</span>
              <input
                type="date"
                value={dateOfBirth}
                onChange={(event) =>
                  setDateOfBirth(event.target.value)
                }
              />
            </label>

            <label className="form-field">
              <span>Ngày tham gia CLB *</span>
              <input
                type="date"
                value={enrollmentDate}
                onChange={(event) =>
                  setEnrollmentDate(event.target.value)
                }
                required
              />
            </label>

            <label className="form-field">
              <span>Đai hiện tại</span>
              <select
                value={currentBeltRankId}
                onChange={(event) =>
                  setCurrentBeltRankId(event.target.value)
                }
              >
                <option value="">Chưa xếp đai</option>
                {sortedBelts.map((belt) => (
                  <option value={belt.id} key={belt.id}>
                    {belt.beltName}
                  </option>
                ))}
              </select>
            </label>

            <label className="form-field">
              <span>Điện thoại</span>
              <input
                type="tel"
                value={phoneNumber}
                onChange={(event) =>
                  setPhoneNumber(event.target.value)
                }
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
                value={address}
                onChange={(event) => setAddress(event.target.value)}
              />
            </label>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section-heading">
            <h2>Thông tin võ thuật</h2>
            <p>
              Có thể bổ sung sau nếu chưa có đầy đủ thông tin.
            </p>
          </div>

          <div className="form-grid">
            <label className="form-field form-field-wide">
              <span>Người giới thiệu</span>
              <input
                value={introducedBy}
                onChange={(event) =>
                  setIntroducedBy(event.target.value)
                }
              />
            </label>

            <label className="form-field form-field-wide">
              <span>Ghi chú hồ sơ võ thuật</span>
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

        {submitError && (
          <div className="form-error">
            <AlertCircle size={20} />
            <span>{submitError}</span>
          </div>
        )}

        <div className="form-actions">
          <Link className="secondary-button" to="/students">
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
                Lưu Môn sinh
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  )
}