# VovinamERP Business Rule Book

## 1. Organization Rules

BR-ORG-001: Mọi dữ liệu nghiệp vụ phải thuộc về một tenant.

BR-ORG-002: Mỗi tenant có thể có nhiều organization.

BR-ORG-003: Organization được tổ chức dạng cây cha-con.

BR-ORG-004: OrganizationType gồm Federation, Province, Club, Branch.

BR-ORG-005: Không xóa organization, chỉ archive.

---

## 2. Student Rules

BR-STU-001: Mỗi môn sinh phải gắn với một Person.

BR-STU-002: Môn sinh phải thuộc đúng một tenant.

BR-STU-003: Mã môn sinh/member number do hệ thống tự sinh.

BR-STU-004: Họ tên bắt buộc.

BR-STU-005: Ngày sinh bắt buộc.

BR-STU-006: Ngày nhập môn bắt buộc.

BR-STU-007: Môn sinh dưới 18 tuổi phải có người giám hộ.

BR-STU-008: Không xóa môn sinh, chỉ archive.

BR-STU-009: Môn sinh có thể đăng ký học nhiều lớp cùng lúc.

BR-STU-010: Môn sinh chỉ có một cấp đai hiện tại.

BR-STU-011: Khi thay đổi cấp đai, phải ghi vào lịch sử cấp đai.

BR-STU-012: Lịch sử cấp đai không được xóa.

BR-STU-013: Hồ sơ môn sinh được phép lưu thành tích, khen thưởng, kỷ luật và chứng chỉ.

BR-STU-014: VovinamERP không tổ chức thi đấu, chỉ lưu thành tích thi đấu vào hồ sơ.

---

## 3. Instructor Rules

BR-INS-001: Mỗi HLV phải gắn với một Person.

BR-INS-002: Một HLV có thể dạy nhiều lớp.

BR-INS-003: Một HLV có thể tham gia nhiều buổi học.

BR-INS-004: HLV của một lớp không cố định theo lớp, mà được ghi nhận theo từng buổi học.

BR-INS-005: Không xóa HLV nếu đã có lịch sử giảng dạy, chỉ archive.

---

## 4. Training Class Rules

BR-CLS-001: Một lớp học thuộc về một tenant.

BR-CLS-002: Một lớp học thuộc về một organization/branch.

BR-CLS-003: Một lớp có thể có nhiều môn sinh.

BR-CLS-004: Một môn sinh có thể học nhiều lớp.

BR-CLS-005: Quan hệ môn sinh - lớp được lưu qua bảng student_class_enrollments.

BR-CLS-006: Khi môn sinh nghỉ lớp, không xóa enrollment, chỉ cập nhật trạng thái/kết thúc.

BR-CLS-007: Lớp học không bắt buộc có HLV cố định.

---

## 5. Training Session Rules

BR-SES-001: Buổi học thuộc về một lớp học.

BR-SES-002: Một buổi học có thể có nhiều HLV.

BR-SES-003: HLV của buổi học được lưu qua bảng session_instructors.

BR-SES-004: Chỉ được điểm danh khi buổi học đã được mở.

BR-SES-005: Buổi học đã đóng thì không được chỉnh sửa điểm danh nếu không có quyền đặc biệt.

BR-SES-006: Buổi học có thể lưu giáo án, ghi chú, hình ảnh hoặc tài liệu.

---

## 6. Attendance Rules

BR-ATT-001: Mỗi buổi học có một phiếu điểm danh.

BR-ATT-002: Mỗi môn sinh trong buổi học có một dòng điểm danh.

BR-ATT-003: Trạng thái điểm danh mặc định gồm Present, Late, Excused, Absent.

BR-ATT-004: Không được điểm danh trùng một môn sinh trong cùng một buổi học.

BR-ATT-005: Chuyên cần được tính dựa trên số buổi có mặt, đi trễ và vắng.

BR-ATT-006: Nghỉ có phép và nghỉ không phép phải được phân biệt.

---

## 7. Finance Rules

BR-FIN-001: Học phí tính theo tháng.

BR-FIN-002: Mỗi môn sinh có thể có nhiều khoản học phí theo từng tháng.

BR-FIN-003: Phiếu thu đã xác nhận không được sửa nội dung chính.

BR-FIN-004: Nếu cần điều chỉnh phiếu thu, phải tạo bản ghi điều chỉnh.

BR-FIN-005: Không xóa lịch sử thanh toán.

BR-FIN-006: Một khoản học phí có thể thanh toán một lần hoặc nhiều lần.

BR-FIN-007: Học phí có thể được miễn giảm nếu người dùng có quyền.

BR-FIN-008: Học giữa tháng xử lý theo quyết định của CLB và được ghi chú trong hóa đơn.

---

## 8. Belt & Promotion Rules

BR-BEL-001: Cấp đai được quản lý bằng BeltRank, không lưu text tự do.

BR-BEL-002: Mỗi BeltRank có level để xác định thứ tự.

BR-BEL-003: BeltRank có thể được kích hoạt hoặc ngưng sử dụng.

BR-PRM-001: Một kỳ thi lên đai thuộc về một tenant.

BR-PRM-002: Một môn sinh có thể thi lại nhiều lần.

BR-PRM-003: Thi trượt vẫn phải lưu lịch sử.

BR-PRM-004: Kết quả thi lên đai không được xóa.

BR-PRM-005: Điều kiện dự thi gồm thời gian tập luyện, tỷ lệ chuyên cần, học phí và xác nhận của HLV.

BR-PRM-006: Khi thi đạt, hệ thống cập nhật cấp đai hiện tại và ghi lịch sử cấp đai.

BR-PRM-007: Khi thi không đạt, hệ thống chỉ ghi kết quả, không thay đổi cấp đai hiện tại.

---

## 9. Achievement Rules

BR-ACH-001: Thành tích có thể gắn với môn sinh hoặc HLV.

BR-ACH-002: Thành tích chỉ là hồ sơ lưu trữ, không tổ chức thi đấu trong hệ thống.

BR-ACH-003: Thành tích có thể đính kèm hình ảnh, giấy chứng nhận hoặc file minh chứng.

BR-ACH-004: Không xóa thành tích đã xác nhận, chỉ archive.

---

## 10. Timeline Rules

BR-TLN-001: Mọi nghiệp vụ quan trọng phải sinh timeline.

BR-TLN-002: Timeline không thay thế audit log.

BR-TLN-003: Timeline dùng để hiển thị lịch sử nghiệp vụ cho người dùng.

BR-TLN-004: Audit log dùng để kiểm tra thay đổi dữ liệu.

BR-TLN-005: Timeline không được xóa bởi người dùng thông thường.

---

## 11. Attachment Rules

BR-ATTACH-001: File đính kèm phải thuộc về một entity nghiệp vụ.

BR-ATTACH-002: File có thể gắn với môn sinh, HLV, chứng chỉ, biên lai hoặc thành tích.

BR-ATTACH-003: Không cho phép upload file vượt quá giới hạn cấu hình.

BR-ATTACH-004: File nhạy cảm chỉ người có quyền mới xem được.

---

## 12. User & Permission Rules

BR-USR-001: Mỗi user phải gắn với một tenant.

BR-USR-002: Một user có thể có nhiều role.

BR-USR-003: Một role có thể có nhiều permission.

BR-USR-004: Quyền được kiểm tra theo action, không theo menu.

BR-USR-005: Người dùng bị khóa không được đăng nhập.

---

## 13. Multi-Tenant Rules

BR-TEN-001: Tất cả bảng nghiệp vụ phải có tenant_id.

BR-TEN-002: Người dùng tenant này không được xem dữ liệu tenant khác.

BR-TEN-003: API phải luôn lọc dữ liệu theo tenant hiện tại.

BR-TEN-004: Seed data mặc định phải theo từng tenant.

BR-TEN-005: SuperAdmin có thể xem nhiều tenant nếu được phân quyền.