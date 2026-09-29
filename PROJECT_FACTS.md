# DỮ LIỆU THỰC TẾ DỰ ÁN (PROJECT FACTS)
**Dự án:** IT Career Platform – Hệ thống nền tảng nghề nghiệp IT tích hợp ATS và AI  
**Học phần:** Quản lý Dự án Công nghệ Thông tin (Nhóm 9)  
**Nhánh kiểm tra:** `dev-kiet`  
**Repository:** `https://github.com/Ht-Hoat/IT_Career_Platform` (tiền thân: `Ht-Hoat/ATS-RecruitmentSystem`)  
**Ngày kiểm toán dữ liệu:** 30/09/2026  

---

## 1. THÔNG TIN HẠ TẦNG VÀ CÔNG NGHỆ

| Thành phần | Công nghệ / Phiên bản thực tế | Nguồn kiểm chứng trong Repo |
| :--- | :--- | :--- |
| **Framework Web** | .NET 10.0 (Blazor Server SSR - Static Server-Side Rendering) | `ITCareerPlatform.Web.csproj`, `Program.cs` |
| **ORM / Data Access** | Entity Framework Core 10.0 (Code First, Migration, SQLite InMemory) | `AppDbContext.cs`, `Migrations/`, `TestDb.cs` |
| **Cơ sở dữ liệu Production** | Microsoft SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`) | `docker-compose.yml`, `appsettings.json` |
| **Cơ sở dữ liệu Test** | SQLite In-Memory (`Microsoft.Data.Sqlite`) với Custom Function hạ chữ tiếng Việt | `TestDb.cs`, `N2_TEST_BUILD_REPORT.md` |
| **Khung kiểm thử (Testing)** | xUnit v2.8.2, FluentAssertions, Coverlet | `ITCareerPlatform.Tests.csproj` |
| **Trí tuệ nhân tạo (AI)** | Google Gemini API (Gemini 1.5/2.0 Flash REST API) | `AiService.cs` (`GeminiAiService`) |
| **Cơ chế dự phòng AI** | Offline Heuristic Matcher (thuật toán đối sánh tập từ khóa TechStack tất định) | `AiService.cs` (`HeuristicAiService`), `AiServiceTests.cs` |
| **Đóng gói & Triển khai** | Docker Engine, Docker Compose multi-stage build (.NET 10 SDK & ASP.NET 10 runtime) | `Dockerfile`, `docker-compose.yml` |
| **Bảo mật & Xác thực** | Cookie Authentication (`Microsoft.AspNetCore.Authentication.Cookies`), BCrypt.Net-Next | `Program.cs`, `Services.cs` (`AuthService`) |
| **Quét mã độc tệp tải lên** | `CvScanner` (chặn Magic Bytes MZ/ELF tệp thực thi và chữ ký kiểm thử virus EICAR) | `CvUtilities.cs`, `ProfileServiceTests.cs` |
| **Bảo vệ dữ liệu cá nhân** | Nghị định 13/2023/NĐ-CP: `AiConsentGate`, `AiConsentAt`, `AiConsentVersion` | `Models.cs`, `AiConsentTests.cs`, `HUONG_DAN_CHAY.md` |
| **Xử lý thời gian** | Chuẩn hóa toàn bộ thời gian lưu trữ ở UTC, hiển thị chuyển đổi sang giờ Việt Nam (UTC+7) | `UiHelpers.cs` (`Ui.ToVietnamTime`), `VietnamTimeTests.cs` |
| **Xử lý Email & Lịch hẹn** | Hàng đợi `EmailOutbox` quét định kỳ 30s bởi tiến trình nền, sinh tệp lịch chuẩn `.ics` | `OutboxSender.cs`, `StatusEmailComposer.cs`, `EmailOutboxTests.cs` |

---

## 2. THÀNH VIÊN VÀ ĐÓNG GÓP THỰC TẾ TRÊN GIT

Kiểm tra toàn bộ Git history từ commit gốc `7871ebb` đến HEAD `dev-kiet`:

| Thành viên / Git Author | Email | Đóng góp và Phạm vi thực tế | Scrum Role ghi nhận |
| :--- | :--- | :--- | :--- |
| **Ht-Hoat** (Hà Thúc Hoạt) | `damm20005@gmail.com` | - Chủ sở hữu repository `Ht-Hoat/IT_Career_Platform`<br>- Khởi tạo nền tảng v2 (commit `7871ebb`, 56 files, 4546 lines)<br>- Merge các Pull Request tích hợp<br>- Bổ sung tính năng HR registration, manual reset password, international job portals | **Product Owner / Lead Dev** *(dựa trên quyền repo và commit kiến trúc gốc)* |
| **Nguyễn Viết Hùng** (`nvhung-28`, `hungnv-2811`) | `nguyenviethung281105@gmail.com` | - 32 commits chuyên sâu về Backend và Refactoring<br>- Nhóm task N1: N1.A (đổi MK), N1.B (internal note), N1.C (câu hỏi PV AI), N1.D (kinh nghiệm), N1.E (lịch PV), N1.F (pipeline filter), N1.G (stats dashboard)<br>- Nhóm task P: P0 (UTC, reset PW), P1-1..5 (Company, SelfCheck, Email Outbox, StatusFlow, Ownership), P2-1..3 (Paging/CSV, CvStorage, Antiforgery/Consent) | **Core Backend Developer** |
| **Võ Nguyên Anh Kiệt** (`Vo Nguyen Anh Kiet`) | `anhkiet0977697765@gmail.com` | - Tác giả nhánh `dev-kiet`<br>- Khắc phục lỗi CS1061 `AppDbContext`, cấu hình SQL Server Express<br>- Nhóm task N2: N2.C (Location case-insensitive), N2.D (HTML native validation & vi-validate), N2.H (CV templates route)<br>- Hoàn thiện toàn bộ giao diện ATS Sprint 4: UI-3 đến UI-13, dashboard KPI links, report forms<br>- Tác giả các tài liệu kiểm định: `BAO_CAO_KIEM_THU.md`, `N2_TEST_BUILD_REPORT.md`, `BAO_CAO_TRIEN_KHAI_UI_ATS.md`, `BAO_CAO_TRIEN_KHAI_UI_ATS_LAN_2.md` | **Frontend / QA & Test Developer** |
| **Thành viên 4** | `[CHƯA XÁC ĐỊNH]` | Chưa có commit hoặc tài liệu định danh riêng trong Git repository | `[CHƯA XÁC ĐỊNH]` *(Ghi nhận trong MISSING_DATA.md)* |
| **Thành viên 5** | `[CHƯA XÁC ĐỊNH]` | Chưa có commit hoặc tài liệu định danh riêng trong Git repository | `[CHƯA XÁC ĐỊNH]` *(Ghi nhận trong MISSING_DATA.md)* |

---

## 3. CƠ SỞ DỮ LIỆU & THỰC THỂ (12 ENTITIES CHÍNH)

| STT | Tên Entity | Mục đích nghiệp vụ | Bảng CSDL tương ứng |
| :---: | :--- | :--- | :--- |
| 1 | `User` | Tài khoản người dùng (Admin, Mentor, Sinh viên), mật khẩu băm BCrypt, SecurityStamp, cờ PendingApproval | `Users` |
| 2 | `Role` | Vai trò hệ thống (1: Admin, 2: Mentor, 3: SinhVienIT) | `Roles` |
| 3 | `Company` | Công ty bảo trợ tin tuyển dụng (Name, Website, Description, Address) | `Companies` |
| 4 | `Job` | Tin tuyển dụng IT (Category, TechStack, Level, EmploymentType, Deadline, SalaryMin/Max, Status) | `Jobs` |
| 5 | `CandidateProfile` | Hồ sơ sinh viên (GitHub, LinkedIn, Portfolio, TechSkillTags, YearsOfExperience, CV data/storage key, AiConsent) | `CandidateProfiles` |
| 6 | `Application` | Đơn ứng tuyển (JobId, CandidateProfileId, CvSnapshot, Status, AiScore, HrScore, InterviewAt, InternalNote) | `Applications` |
| 7 | `ApplicationStatusHistory` | Lịch sử timeline đổi trạng thái đơn (FromStatus, ToStatus, ChangedByUserId, ChangedAt) | `ApplicationStatusHistories` |
| 8 | `EmailOutbox` | Hàng đợi gửi email tự động (ToEmail, Subject, Body, Attachment .ics, SentAt, Attempts, MaxAttempts=5) | `EmailOutbox` |
| 9 | `SelfCheck` | Sinh viên tự kiểm tra độ phù hợp trước khi nộp (hạn mức 5 lượt/ngày, lưu điểm, điểm mạnh, thiếu sót, lộ trình) | `SelfChecks` |
| 10 | `Notification` | Thông báo chuông cho sinh viên khi đổi trạng thái đơn (Title, Message, Link, IsRead) | `Notifications` |
| 11 | `AuditLog` | Nhật ký kiểm toán thao tác quản trị của Admin (Action, TableName, Details, Timestamp) | `AuditLogs` |
| 12 | `InterviewQuestionSnapshot` | Lưu lịch sử các bộ câu hỏi luyện phỏng vấn AI cho từng đơn ứng tuyển | `InterviewQuestionSnapshots` |

---

## 4. DANH MỤC USER STORIES ĐÃ XÁC MINH TRONG CODEBASE

### 4.1. Phân hệ Quản trị & Cốt lõi (Core & Admin)
- **ATS-01:** Quản lý danh sách tài khoản người dùng (`/users`), khóa/mở khóa tài khoản, phân quyền Role.
- **ATS-02:** Nhật ký kiểm toán `AuditLogs` ghi vết thao tác nhạy cảm (đổi Role, khóa user, v.v.).
- **ATS-03 / EXT-01:** Xác thực đăng nhập qua Cookie Auth, đăng ký tài khoản Sinh viên IT tự do (`/register`), đăng ký HR có kiểm duyệt (`/register-hr`).
- **UI-3:** Trang Admin duyệt tài khoản HR (`/users/pending`) kèm badge số lượng và banner thông báo.
- **P0-3 / UI-1:** Admin đặt lại mật khẩu ngẫu nhiên an toàn hoặc nhập tay, gửi email tự động hoặc hiển thị hộp thoại một lần, bật cờ `MustChangePassword`.

### 4.2. Phân hệ Tin tuyển dụng & Định vị (Jobs Management)
- **ATS-04:** Đăng tin tuyển dụng IT với 3 trường chuẩn hóa: `Category` (8 danh mục), `TechStack` (chuỗi công nghệ phân tách chuẩn), `Level` (4 cấp bậc).
- **ATS-05:** Cập nhật và đóng tin tuyển dụng (chỉ Mentor tạo tin mới có quyền sửa/đóng tin của mình; Admin chỉ có quyền xem).
- **ATS-06:** Quản lý trạng thái mở/đóng tin (`Open`/`Closed`). Chặn nộp đơn khi tin đã đóng hoặc đã quá hạn `Deadline`.
- **ATS-07:** Tìm kiếm và đa bộ lọc việc làm: Category, TechStack (case-insensitive), Location (case-insensitive qua SQLite & SQL Server), Level, EmploymentType (`Onsite`, `Remote`, `Hybrid`), Lương.
- **P1-1:** Gắn thực thể `Company` chính thức vào từng tin tuyển dụng.
- **UI-13:** Trang xem chi tiết JD chỉ đọc cho Admin/Mentor (`/jobs/{id}/detail`).

### 4.3. Phân hệ Hồ sơ & Ứng tuyển của Sinh viên (Candidate Journey)
- **ATS-08:** Hồ sơ nghề nghiệp IT với 4 trường chuyên sâu: URL GitHub, LinkedIn, Portfolio, TechSkillTags và `YearsOfExperience` (suy ra Level: Fresher, Junior, Middle, Senior).
- **ATS-09 / SEC-01:** Tải lên CV (PDF/DOCX, max 5MB). `CvScanner` quét an toàn chặn tệp thực thi (MZ/ELF) và virus EICAR.
- **ATS-10:** Ứng tuyển việc làm với cơ chế kiểm tra 3 lớp (hồ sơ, CV, thời hạn, trùng lặp) và đóng băng bản chụp CV (`CvDataSnapshot` / `CvStorageKeySnapshot`).
- **P0-4:** Sinh viên có quyền rút đơn ứng tuyển (`Withdrawn`) khi chưa chốt kết quả.
- **P1-2 / ATS-14:** Sinh viên tự đánh giá độ phù hợp trước khi nộp (`SelfCheck`, giới hạn 5 lượt/ngày).
- **N2.H:** Kho tài nguyên mẫu CV IT (`/cv-templates`) và mẹo viết CV.

### 4.4. Phân hệ ATS & Đánh giá AI (Recruiter & AI Scoring)
- **ATS-11:** Mentor xem danh sách ứng viên theo tin tuyển dụng của mình (Admin xem toàn bộ ở chế độ giám sát).
- **ATS-12:** Xem chi tiết hồ sơ ứng viên, tải bản chụp CV gốc.
- **ATS-13:** Đánh giá độ phù hợp bằng Google Gemini API (trả về % Match, Điểm mạnh, Thiếu sót, Lộ trình học tập).
- **ATS-14:** Cơ chế tự động chuyển đổi sang Heuristic Offline Matcher khi không có key, lỗi mạng, timeout hoặc hết quota.
- **ATS-15:** Tự động xếp hạng ứng viên theo % phù hợp và phân màu trực quan (`score-high` >= 70%: Xanh, `score-mid` >= 40%: Vàng, `score-low` < 40%: Đỏ).
- **ATS-16:** Nguyên tắc Human-in-the-loop: Điểm AI chỉ mang tính tham khảo; Mentor chốt điểm cuối cùng (`HrScore`, `HrNote`). `FinalScore` ưu tiên `HrScore`.
- **ATS-17 / P1-4:** Quy trình quản lý trạng thái 5 bước nghiêm ngặt (`ApplicationStatusFlow`) và lưu vết lịch sử timeline (`ApplicationStatusHistories`).
- **N1.B:** Ghi chú nội bộ bí mật của Mentor dành riêng cho ứng viên (`InternalNote`).
- **N1.C / HIST / UI-4:** Sinh câu hỏi luyện phỏng vấn kèm gợi ý (`Hint`) và lưu lịch sử các bộ câu hỏi.
- **N1.E / P1-3:** Lên lịch phỏng vấn, tự động tạo tệp lịch hẹn `.ics` gửi qua hàng đợi `EmailOutbox`.
- **ATS-18 / UI-11:** Dashboard phân tích và thống kê tuyển dụng với Chart.js (phễu tuyển dụng, phân bố điểm, tỷ lệ chuyển đổi).

---

## 5. DỮ LIỆU KIỂM THỬ THỰC TẾ

1. **Giai đoạn Kiểm thử sơ bộ (06/09/2026 - `BAO_CAO_KIEM_THU.md`):**
   - 35 automated unit tests xUnit: 35/35 PASSED (100%).
   - 24 kịch bản kiểm thử nghiệp vụ thủ công: 24/24 PASSED (100%).
   - Phát hiện 2 sai lệch nhỏ: SeedData ứng viên Khoa là Frontend thay vì Backend; ngưỡng lọc màu điểm dùng `> 70` thay vì `>= 70`.
   - 1 tính năng chưa cài đặt: Rate limiting cho AI Evaluation.
2. **Giai đoạn Kiểm tra N2 Verification (13/09/2026 - `N2_TEST_BUILD_REPORT.md`):**
   - 59 automated unit tests xUnit: 59/59 PASSED (100%), thời gian ~21s.
   - Xác minh N2.C (Location case-insensitive), N2.D (native HTML5 validation), N2.H (CV templates navigation).
3. **Giai đoạn Nghiệm thu toàn diện Sprint 4 (20/09/2026 - `BAO_CAO_KIEM_TRA_GIAO_DIEN_ATS.md` & `BAO_CAO_TRIEN_KHAI_UI_ATS_LAN_2.md`):**
   - `dotnet build`: Thành công 0 Warning, 0 Error.
   - `dotnet test`: **415 / 415 tests PASSED (100% xanh)** trong 51s - 59s.
   - Đạt 100% bảo vệ Antiforgery CSRF (`PageFormsHaveEndpointsTests`), duyệt HR (`HrRegistrationApprovalTests`), bảo vệ quyền Mentor/Admin.
