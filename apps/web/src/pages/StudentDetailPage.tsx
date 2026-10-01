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
  getStudentBeltHistory,
  recordStudentBeltResult,
  transitionJuniorYellowBelt,
} from '../lib/studentsApi'
import type { StudentBeltHistoryItem } from '../lib/studentsApi'
import type { BeltRank, Student } from '../lib/types'

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

function hasReachedAge(
  dateOfBirth: string | null,
  age: number,
  onDate: Date,
) {
  if (!dateOfBirth) return false

  const [year, month, day] = dateOfBirth.slice(0, 10).split('-').map(Number)

  if (!year || !month || !day) return false

  const birthdayThisYear = new Date(
    onDate.getFullYear(),
    month - 1,
    day,
  )

  return (
    onDate.getFullYear() - year > age ||
    (onDate.getFullYear() - year === age &&
      onDate >= birthdayThisYear)
  )
}
export default function StudentDetailPage() {
  const { studentId } = useParams()

  const [student, setStudent] = useState<Student | null>(null)
  const [beltHistory, setBeltHistory] = useState<StudentBeltHistoryItem[]>([])
  const [beltRanks, setBeltRanks] = useState<BeltRank[]>([])
  const [showBeltResultForm, setShowBeltResultForm] = useState(false)
  const [beltResultRankId, setBeltResultRankId] = useState('')
  const [beltResultExamDate, setBeltResultExamDate] = useState('')
  const [beltResult, setBeltResult] = useState<1 | 2>(1)
  const [beltResultAwardedDate, setBeltResultAwardedDate] = useState('')
  const [beltResultNote, setBeltResultNote] = useState('')
  const [savingBeltResult, setSavingBeltResult] = useState(false)
  const [beltResultError, setBeltResultError] = useState<string | null>(null)
  const [beltName, setBeltName] = useState('Chưa xếp đai')
  const [transitioningYellowBelt, setTransitioningYellowBelt] = useState(false)
  const [yellowBeltTransitionError, setYellowBeltTransitionError] =
    useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [changingStatus, setChangingStatus] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const currentBelt = student?.currentBeltRankId
    ? beltRanks.find((belt) => belt.id === student.currentBeltRankId)
    : undefined

  const isJuniorYellowBelt = currentBelt?.beltCode === 'HOANG-TN'

  const canTransitionToYellowBelt =
    isJuniorYellowBelt &&
    !!student?.dateOfBirth &&
    hasReachedAge(student.dateOfBirth, 12, new Date())

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

        const [studentResult, beltRankResult] = await Promise.all([
          getStudent(studentId!, controller.signal),
          getBeltRanks(controller.signal),
        ])

        setStudent(studentResult)
        setBeltRanks(beltRankResult)

        const beltHistoryResult = await getStudentBeltHistory(
          studentId!,
          studentResult.tenantId,
          controller.signal,
        )

        setBeltHistory(beltHistoryResult.items)
        const belt = beltRankResult.find(
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

  async function handleTransitionJuniorYellowBelt() {
    if (!student || !studentId || !canTransitionToYellowBelt) return

    const confirmed = window.confirm(
      'Xác nhận chuyển Môn sinh từ Hoàng đai thiếu nhi sang Hoàng đai? Đây là chuyển đổi theo độ tuổi, không phải kết quả thi đai.',
    )

    if (!confirmed) return

    const today = new Date()
    const transitionDate = [
      today.getFullYear(),
      String(today.getMonth() + 1).padStart(2, '0'),
      String(today.getDate()).padStart(2, '0'),
    ].join('-')

    try {
      setTransitioningYellowBelt(true)
      setYellowBeltTransitionError(null)

      await transitionJuniorYellowBelt(studentId, {
        tenantId: student.tenantId,
        transitionDate,
        note: 'Chuyển từ Hoàng đai thiếu nhi sang Hoàng đai khi đủ 12 tuổi.',
        userId: null,
      })

      const updatedStudent = await getStudent(studentId)

      setStudent(updatedStudent)

      const updatedCurrentBelt = beltRanks.find(
        (belt) => belt.id === updatedStudent.currentBeltRankId,
      )

      setBeltName(updatedCurrentBelt?.beltName ?? 'Chưa xếp đai')
    } catch (err) {
      console.error(err)
      setYellowBeltTransitionError(
        err instanceof Error
          ? err.message
          : 'Không thể chuyển sang Hoàng đai. Vui lòng thử lại.',
      )
    } finally {
      setTransitioningYellowBelt(false)
    }
  }

  async function handleRecordBeltResult() {
    if (!student || !studentId) return

    if (!beltResultRankId) {
      setBeltResultError('Vui lòng chọn cấp đai dự thi.')
      return
    }

    if (!beltResultExamDate) {
      setBeltResultError('Vui lòng chọn ngày thi.')
      return
    }

    if (beltResult === 1 && !beltResultAwardedDate) {
      setBeltResultError(
        'Kết quả Đạt cần có ngày công nhận cấp đai.',
      )
      return
    }

    if (
      beltResult === 1 &&
      beltResultAwardedDate < beltResultExamDate
    ) {
      setBeltResultError(
        'Ngày công nhận không được trước ngày thi.',
      )
      return
    }

    try {
      setSavingBeltResult(true)
      setBeltResultError(null)

      await recordStudentBeltResult(studentId, {
        tenantId: student.tenantId,
        beltRankId: beltResultRankId,
        examDate: beltResultExamDate,
        result: beltResult,
        awardedDate:
          beltResult === 1 ? beltResultAwardedDate : null,
        note: beltResultNote.trim() || null,
        userId: null,
      })

      const [updatedStudent, updatedHistory] = await Promise.all([
        getStudent(studentId),
        getStudentBeltHistory(studentId, student.tenantId),
      ])

      setStudent(updatedStudent)
      setBeltHistory(updatedHistory.items)

      const currentBelt = beltRanks.find(
        (item) => item.id === updatedStudent.currentBeltRankId,
      )

      setBeltName(currentBelt?.beltName ?? 'Chưa xếp đai')

      setShowBeltResultForm(false)
      setBeltResultRankId('')
      setBeltResultExamDate('')
      setBeltResult(1)
      setBeltResultAwardedDate('')
      setBeltResultNote('')
    } catch (err) {
      console.error(err)

      setBeltResultError(
        err instanceof Error
          ? err.message
          : 'Không thể ghi kết quả thi đai.',
      )
    } finally {
      setSavingBeltResult(false)
    }
  }
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

          {canTransitionToYellowBelt && (
            <section className="detail-card">
              <div className="form-section-heading">
                <h2>Chuyển sang Hoàng đai</h2>
                <p>
                  Môn sinh đang mang Hoàng đai thiếu nhi và đã đủ 12 tuổi.
                  Sau khi hoàn thiện, kiểm tra hồ sơ, có thể chuyển sang Hoàng đai.
                  Đây là chuyển đổi theo độ tuổi, không phải một lần thi đai.
                </p>
              </div>

              {yellowBeltTransitionError && (
                <div className="form-error">
                  <AlertCircle size={18} />
                  <span>{yellowBeltTransitionError}</span>
                </div>
              )}

              <div className="form-actions">
                <button
                  className="primary-button"
                  type="button"
                  disabled={transitioningYellowBelt}
                  onClick={() => void handleTransitionJuniorYellowBelt()}
                >
                  {transitioningYellowBelt
                    ? 'Đang chuyển...'
                    : 'Chuyển sang Hoàng đai'}
                </button>
              </div>
            </section>
          )}
          <section className="form-section belt-history-section">
            <div className="belt-history-heading">
              <div className="form-section-heading">
                <h2>Lịch sử đai</h2>
                <p>
                  Theo dõi các lần thi đai, kết quả đạt hoặc trượt và quá trình
                  thay đổi cấp đai của Môn sinh.
                </p>
              </div>

              <button
                className="secondary-button"
                type="button"
                onClick={() => {
                  setShowBeltResultForm((current) => !current)
                  setBeltResultError(null)
                }}
              >
                {showBeltResultForm
                  ? 'Đóng'
                  : '+ Ghi kết quả thi đai'}
              </button>
            </div>

            {showBeltResultForm && (
              <div className="belt-result-form">
                {beltResultError && (
                  <div className="form-error">
                    <AlertCircle size={18} />
                    <span>{beltResultError}</span>
                  </div>
                )}

                <div className="form-grid">
                  <label className="form-field">
                    <span>Cấp đai dự thi</span>
                    <select
                      value={beltResultRankId}
                      onChange={(event) =>
                        setBeltResultRankId(event.target.value)
                      }
                    >
                      <option value="">Chọn cấp đai</option>

                      {beltRanks
                        .filter((belt) => belt.isActive)
                        .sort((a, b) => a.level - b.level)
                        .map((belt) => (
                          <option key={belt.id} value={belt.id}>
                            {belt.beltName}
                          </option>
                        ))}
                    </select>
                  </label>

                  <label className="form-field">
                    <span>Ngày thi</span>
                    <input
                      type="date"
                      value={beltResultExamDate}
                      onChange={(event) =>
                        setBeltResultExamDate(event.target.value)
                      }
                    />
                  </label>

                  <label className="form-field">
                    <span>Kết quả</span>
                    <select
                      value={beltResult}
                      onChange={(event) => {
                        const result = Number(event.target.value) as 1 | 2

                        setBeltResult(result)

                        if (result === 2) {
                          setBeltResultAwardedDate('')
                        }
                      }}
                    >
                      <option value={1}>Đạt</option>
                      <option value={2}>Trượt</option>
                    </select>
                  </label>

                  {beltResult === 1 && (
                    <label className="form-field">
                      <span>Ngày công nhận</span>
                      <input
                        type="date"
                        min={beltResultExamDate || undefined}
                        value={beltResultAwardedDate}
                        onChange={(event) =>
                          setBeltResultAwardedDate(event.target.value)
                        }
                      />
                    </label>
                  )}

                  <label className="form-field form-field-wide">
                    <span>Ghi chú</span>
                    <textarea
                      rows={3}
                      value={beltResultNote}
                      placeholder="Ghi chú thêm nếu cần"
                      onChange={(event) =>
                        setBeltResultNote(event.target.value)
                      }
                    />
                  </label>
                </div>

                <div className="form-actions">
                  <button
                    className="primary-button"
                    type="button"
                    disabled={savingBeltResult}
                    onClick={() => void handleRecordBeltResult()}
                  >
                    {savingBeltResult
                      ? 'Đang lưu...'
                      : 'Lưu kết quả'}
                  </button>

                  <button
                    className="secondary-button"
                    type="button"
                    disabled={savingBeltResult}
                    onClick={() => {
                      setShowBeltResultForm(false)
                      setBeltResultError(null)
                    }}
                  >
                    Hủy
                  </button>
                </div>
              </div>
            )}

            {beltHistory.length === 0 ? (
              <div className="belt-history-empty">
                <strong>Chưa có lịch sử đai</strong>
                <span>
                  Các lần thi đai của Môn sinh sẽ được hiển thị tại đây.
                </span>
              </div>
            ) : (
              <div className="belt-history-list">
                {beltHistory.map((item) => (
                  <article className="belt-history-item" key={item.id}>
                    <div className="belt-history-main">
                      <div>
                        <span className="belt-history-label">Cấp đai</span>
                        <strong>{item.beltName}</strong>
                      </div>

                      <span
                        className={`belt-result ${
                          Number(item.result) === 1
                            ? 'belt-result-passed'
                            : 'belt-result-failed'
                        }`}
                      >
                        {Number(item.result) === 1 ? 'Đạt' : 'Trượt'}
                      </span>
                    </div>

                    <div className="belt-history-details">
                      <div>
                        <span>Ngày thi</span>
                        <strong>{formatDate(item.examDate)}</strong>
                      </div>

                      <div>
                        <span>Ngày công nhận</span>
                        <strong>
                          {item.awardedDate
                            ? formatDate(item.awardedDate)
                            : 'Không có'}
                        </strong>
                      </div>
                    </div>

                    {item.note && (
                      <div className="belt-history-note">
                        <span>Ghi chú</span>
                        <p>{item.note}</p>
                      </div>
                    )}
                  </article>
                ))}
              </div>
            )}
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
