# VovinamERP Database Blueprint

## 1. Architecture Decision

VovinamERP sử dụng kiến trúc Multi-Tenant.

Một hệ thống có thể phục vụ nhiều CLB/võ đường.
Mỗi tenant chỉ nhìn thấy dữ liệu của tenant mình.

---

## 2. Core Foundation Tables

### tenants
Đại diện cho một khách hàng/tổ chức sử dụng hệ thống.

Ví dụ:
- CLB Lê Đô
- CLB Biên Hòa
- Trung tâm Vovinam Đồng Nai

Cột chính:
- id
- code
- name
- status
- created_at_utc
- updated_at_utc
- archived_at_utc

---

### organizations
Đơn vị tổ chức dạng cây.

Ví dụ:
- Liên đoàn
- Tỉnh/Thành
- CLB
- Chi nhánh/Cơ sở

Cột chính:
- id
- tenant_id
- parent_id
- code
- name
- organization_type
- address
- phone_number
- email
- status

---

## 3. People Domain

### persons
Thông tin con người dùng chung.

Một person có thể là:
- Môn sinh
- HLV
- Phụ huynh
- Nhân viên
- Người dùng hệ thống

Cột chính:
- id
- tenant_id
- code
- full_name
- gender
- date_of_birth
- phone_number
- email
- address
- avatar_url

---

### students
Hồ sơ môn sinh.

Cột chính:
- id
- tenant_id
- person_id
- member_number
- organization_id
- current_belt_rank_id
- enrollment_date
- status
- martial_name
- martial_profile_note

---

### instructors
Hồ sơ huấn luyện viên.

Cột chính:
- id
- tenant_id
- person_id
- instructor_number
- organization_id
- status

---

### guardians
Phụ huynh/người giám hộ.

Cột chính:
- id
- tenant_id
- person_id
- relationship_note

---

### student_guardians
Liên kết môn sinh và phụ huynh.

Cột chính:
- id
- tenant_id
- student_id
- guardian_id
- relationship
- is_primary

---

## 4. Belt Domain

### belt_ranks
Danh mục cấp đai.

Cột chính:
- id
- tenant_id
- code
- name
- level
- color
- description
- is_active

---

### belt_requirements
Điều kiện lên đai.

Cột chính:
- id
- tenant_id
- from_belt_rank_id
- to_belt_rank_id
- minimum_months
- minimum_attendance_rate
- description

---

### student_belt_histories
Lịch sử cấp đai của môn sinh.

Cột chính:
- id
- tenant_id
- student_id
- belt_rank_id
- awarded_date
- note

---

## 5. Training Domain

### training_classes
Lớp học.

Cột chính:
- id
- tenant_id
- organization_id
- code
- name
- main_instructor_id
- status

---

### training_sessions
Buổi học.

Cột chính:
- id
- tenant_id
- training_class_id
- session_date
- start_time
- end_time
- instructor_id
- status

---

### attendance_records
Phiếu điểm danh.

Cột chính:
- id
- tenant_id
- training_session_id
- created_by

---

### attendance_details
Chi tiết điểm danh từng môn sinh.

Cột chính:
- id
- tenant_id
- attendance_record_id
- student_id
- status
- note

---

## 6. Finance Domain

### tuition_plans
Gói học phí.

### tuition_invoices
Khoản học phí phải thu.

### tuition_payments
Khoản đã thanh toán.

### receipts
Biên lai.

---

## 7. Promotion Domain

### promotion_exams
Kỳ thi lên đai.

### promotion_candidates
Môn sinh đăng ký thi.

### promotion_results
Kết quả thi.

### certificates
Chứng nhận.

---

## 8. Achievement Domain

### achievements
Thành tích của môn sinh hoặc HLV.

Lưu ý:
VovinamERP chỉ lưu thành tích vào hồ sơ.
Không tổ chức thi đấu trong hệ thống này.

---

## 9. System Domain

### users
Tài khoản đăng nhập.

### roles
Vai trò.

### permissions
Quyền thao tác.

### user_roles
Liên kết user và role.

### role_permissions
Liên kết role và permission.

### timeline_events
Dòng thời gian nghiệp vụ.

### audit_logs
Nhật ký thay đổi dữ liệu.

### attachments
File đính kèm.

### notifications
Thông báo trong hệ thống.

---

## 10. Global Database Rules

- Tất cả bảng nghiệp vụ có id dạng UUID.
- Tất cả bảng nghiệp vụ có tenant_id.
- Không xóa dữ liệu nghiệp vụ, chỉ archive.
- Thời gian lưu UTC.
- Tên bảng dùng snake_case.
- Tên cột dùng snake_case.
- Không dùng mã nghiệp vụ làm khóa chính.
- Các bảng chính có code hoặc number để người dùng nhìn thấy.
- Dữ liệu giữa các tenant phải được cô lập.