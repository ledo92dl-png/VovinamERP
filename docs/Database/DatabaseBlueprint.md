# VovinamERP Database Blueprint

## 1. Foundation

### users
Tài khoản đăng nhập hệ thống.

### roles
Vai trò người dùng.

### permissions
Quyền thao tác.

### user_roles
Liên kết user và role.

### role_permissions
Liên kết role và permission.

---

## 2. Organization

### organizations
Đơn vị tổ chức: Liên đoàn, tỉnh, CLB, chi nhánh.

### organization_types
Loại tổ chức.

### clubs
Võ đường/CLB trực tiếp quản lý môn sinh, lớp học, học phí và thi lên đai.

### branches
Cơ sở/địa điểm tập thuộc CLB.

---

## 3. Belt

### belt_ranks
Danh mục cấp đai.

### belt_requirements
Điều kiện lên đai.

---

## 4. Student

### persons
Thông tin con người dùng chung.

### students
Hồ sơ môn sinh.

### guardians
Phụ huynh/người giám hộ.

### student_guardians
Liên kết môn sinh và phụ huynh.

### student_belt_histories
Lịch sử cấp đai của môn sinh.

---

## 5. Instructor

### instructors
Hồ sơ huấn luyện viên.

### instructor_certificates
Chứng chỉ của HLV.

---

## 6. Training

### training_classes
Lớp học.

### training_sessions
Buổi học.

### attendance_records
Phiếu điểm danh của một buổi học.

### attendance_details
Chi tiết điểm danh từng môn sinh.

---

## 7. Finance

### tuition_plans
Gói học phí.

### tuition_invoices
Khoản học phí phải thu.

### tuition_payments
Khoản học phí đã thanh toán.

### receipts
Biên lai.

---

## 8. Promotion

### promotion_exams
Kỳ thi lên đai.

### promotion_candidates
Môn sinh đăng ký thi.

### promotion_results
Kết quả thi lên đai.

### certificates
Chứng nhận.

---

## 9. Achievement

### achievements
Thành tích của môn sinh hoặc HLV.

Lưu ý: VovinamERP chỉ lưu thành tích, không tổ chức thi đấu.

---

## 10. System

### timeline_events
Dòng thời gian nghiệp vụ.

### audit_logs
Nhật ký thay đổi dữ liệu.

### attachments
File đính kèm.

### notifications
Thông báo trong hệ thống.

---

## 11. Quy tắc chung

- Tất cả bảng nghiệp vụ có id dạng UUID.
- Không xóa dữ liệu nghiệp vụ, chỉ archive.
- Các bảng chính có created_at_utc, updated_at_utc, archived_at_utc.
- Dữ liệu thời gian lưu UTC.
- Tên bảng dùng snake_case.
- Tên cột dùng snake_case.
- Các mã nghiệp vụ như student_code, club_code không dùng làm khóa chính.