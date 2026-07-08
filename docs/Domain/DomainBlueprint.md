# VovinamERP Domain Blueprint

## 1. Organization Domain

### Organization
Đại diện cho một đơn vị tổ chức trong hệ thống.

Ví dụ:
- Liên đoàn
- Tỉnh/Thành
- CLB
- Chi nhánh/Cơ sở

### OrganizationType
- Federation
- Province
- Club
- Branch

### Club
Võ đường/CLB trực tiếp quản lý lớp, HLV, môn sinh, học phí và kỳ thi lên đai.

---

## 2. People Domain

### Person
Thông tin con người dùng chung.

Một Person có thể là:
- Môn sinh
- HLV
- Phụ huynh
- Nhân viên
- Người dùng hệ thống

### Student
Hồ sơ môn sinh.

### Instructor
Hồ sơ huấn luyện viên.

### Guardian
Phụ huynh/người giám hộ.

---

## 3. Training Domain

### TrainingClass
Lớp học.

### TrainingSession
Một buổi học cụ thể.

### AttendanceRecord
Phiếu điểm danh của một buổi học.

### AttendanceDetail
Trạng thái điểm danh của từng môn sinh.

Trạng thái:
- Present
- Late
- Excused
- Absent

---

## 4. Belt & Promotion Domain

### BeltRank
Cấp đai.

Ví dụ:
- Lam đai
- Lam I
- Lam II
- Lam III
- Hoàng đai

### StudentBeltHistory
Lịch sử cấp đai của môn sinh.

### PromotionExam
Kỳ thi lên đai.

### PromotionCandidate
Môn sinh đăng ký thi.

### PromotionResult
Kết quả thi lên đai.

### Certificate
Chứng nhận sau khi thi đạt.

---

## 5. Finance Domain

### TuitionPlan
Quy định học phí.

### TuitionInvoice
Khoản học phí phải thu.

### TuitionPayment
Khoản đã thu.

### Receipt
Biên lai.

---

## 6. Achievement Domain

### Achievement
Thành tích của môn sinh hoặc HLV.

Ví dụ:
- Huy chương
- Giấy khen
- Danh hiệu
- Thành tích huấn luyện

Lưu ý:
VovinamERP không tổ chức thi đấu. Hệ thống chỉ lưu thành tích vào hồ sơ.

---

## 7. Timeline Domain

### TimelineEvent
Dòng thời gian nghiệp vụ.

Ví dụ:
- Đăng ký môn sinh
- Chuyển lớp
- Đóng học phí
- Thi lên đai
- Nhận chứng chỉ
- Đạt thành tích

---

## 8. User & Permission Domain

### UserAccount
Tài khoản đăng nhập.

### Role
Vai trò.

### Permission
Quyền thao tác.

Vai trò mặc định:
- SuperAdmin
- ClubOwner
- Secretary
- Instructor
- Accountant

---

## 9. V1 Scope

V1 bao gồm:
- Organization
- Club
- Person
- Student
- Instructor
- Guardian
- TrainingClass
- TrainingSession
- Attendance
- BeltRank
- PromotionExam
- Tuition
- Achievement
- Timeline
- UserAccount
- Role
- Permission

V1 không bao gồm:
- Tổ chức giải đấu
- Bốc thăm
- SIMA
- Chấm điểm thi đấu
- Livestream
- Marketplace
- AI
- Offline mode

---

## 10. Build Order

1. Organization / Club
2. BeltRank
3. Person
4. Student
5. Instructor
6. TrainingClass
7. TrainingSession
8. Attendance
9. Tuition
10. PromotionExam
11. Achievement
12. Timeline
13. Authentication
14. Dashboard