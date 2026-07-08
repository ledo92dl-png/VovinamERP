# Student Profile Blueprint

## 1. Mục tiêu

Student Profile là hồ sơ trung tâm của môn sinh trong VovinamERP.

Hồ sơ này dùng cho:
- Quản lý thông tin cá nhân
- Quản lý quá trình tập luyện
- Quản lý cấp đai
- Quản lý điểm danh
- Quản lý học phí
- Quản lý thành tích
- Quản lý chứng chỉ
- Quản lý hồ sơ võ đạo

---

## 2. Nhóm thông tin cá nhân

- StudentCode
- FullName
- NickName
- Gender
- DateOfBirth
- CitizenId
- PhoneNumber
- Email
- Address
- AvatarUrl

---

## 3. Nhóm thông tin võ đạo

- EnrollmentDate
- ClubId
- BranchId
- TrainingClassId
- DirectInstructorId
- CurrentBeltRankId
- MartialName
- IntroducedBy
- MartialProfileNote

---

## 4. Nhóm trạng thái học tập

StudentStatus:
- Trial
- Active
- Paused
- Reserved
- Left
- Graduated
- Archived

---

## 5. Hồ sơ phụ huynh / người liên hệ

- GuardianFullName
- Relationship
- GuardianPhoneNumber
- GuardianEmail
- GuardianAddress
- EmergencyContactName
- EmergencyContactPhone

---

## 6. Hồ sơ sức khỏe

- HeightCm
- WeightKg
- BloodType
- MedicalCondition
- AllergyNote
- InjuryHistory
- TrainingRestriction
- CoachMedicalNote

---

## 7. Hồ sơ cấp đai

Không lưu cấp đai bằng text.

Dữ liệu liên quan:
- CurrentBeltRankId
- StudentBeltHistory
- PromotionExam
- PromotionResult
- Certificate

---

## 8. Hồ sơ thành tích

VovinamERP không tổ chức thi đấu.

Hệ thống chỉ lưu thành tích vào hồ sơ:
- AchievementTitle
- AchievementType
- EventName
- AwardDate
- Medal
- Description
- AttachmentUrl

Ví dụ:
- HCV Giải trẻ TP.HCM 2026
- Giấy khen HLV xuất sắc
- Chứng nhận hoàn thành khóa tập huấn

---

## 9. Hồ sơ khen thưởng / kỷ luật

Reward:
- Title
- Date
- Reason
- AttachmentUrl

Discipline:
- Title
- Date
- Reason
- Resolution
- AttachmentUrl

---

## 10. Tài liệu đính kèm

- Ảnh cá nhân
- CCCD
- Đơn đăng ký
- Giấy khám sức khỏe
- Chứng chỉ
- Giấy khen
- File khác

---

## 11. Timeline

Mọi sự kiện quan trọng phải sinh Timeline:
- Đăng ký môn sinh
- Chuyển lớp
- Tạm nghỉ
- Quay lại học
- Lên đai
- Đóng học phí
- Đạt thành tích
- Nhận chứng chỉ
- Khen thưởng
- Kỷ luật

---

## 12. Quy tắc nghiệp vụ

BR-STU-001: Họ tên bắt buộc.

BR-STU-002: Ngày sinh bắt buộc.

BR-STU-003: Ngày nhập môn bắt buộc.

BR-STU-004: Môn sinh phải thuộc một CLB.

BR-STU-005: Mã môn sinh tự sinh, không nhập tay.

BR-STU-006: Không xóa môn sinh, chỉ Archive.

BR-STU-007: Nếu môn sinh dưới 18 tuổi thì phải có người giám hộ.

BR-STU-008: Cấp đai hiện tại phải lấy từ BeltRank.

BR-STU-009: Khi thay đổi cấp đai phải ghi vào StudentBeltHistory.

BR-STU-010: Thành tích chỉ lưu hồ sơ, không tổ chức thi đấu trong VovinamERP.

---

## 13. Build Order cho Student

1. StudentStatus
2. Student Entity
3. StudentCreatedEvent
4. StudentUpdatedEvent
5. StudentArchivedEvent
6. StudentErrors
7. Student Repository Interface
8. Student EF Configuration
9. Student API
10. Student UI