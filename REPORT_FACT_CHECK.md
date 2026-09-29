# BẢNG ĐỐI CHIẾU VÀ KIỂM CHỨNG DỮ LIỆU BÁO CÁO (REPORT FACT CHECK)
**Dự án:** IT Career Platform – Hệ thống nền tảng nghề nghiệp IT tích hợp ATS và AI  
**Nhánh Git:** `dev-kiet`  
**Ngày kiểm toán:** 30/09/2026  
**Nguyên tắc thẩm định:**  
- **VERIFIED:** Đã kiểm chứng chính xác 100% qua mã nguồn, Git commit, test run hoặc tài liệu kỹ thuật trong kho lưu trữ.  
- **NEED EVIDENCE:** Khẳng định hợp lý về mặt lý thuyết/quy trình nhưng chưa tìm thấy tệp minh chứng trực tiếp (như Jira export, biên bản họp) trong repository.  
- **UNVERIFIED:** Không có cơ sở dữ liệu trong repository, đã được loại bỏ hoặc gắn cờ cảnh báo rõ ràng.  

---

## BẢNG KIỂM TOÁN CHI TIẾT

| # | Luận điểm / Khẳng định trong báo cáo | Nguồn kiểm chứng thực tế trong Repository | Trạng thái | Ghi chú & Chi tiết kiểm chứng |
| :---: | :--- | :--- | :---: | :--- |
| **1** | Sử dụng công nghệ .NET 10 Blazor Server SSR | `src/ITCareerPlatform.Web/ITCareerPlatform.Web.csproj`, `Program.cs` | **VERIFIED** | TargetFramework `net10.0`, sử dụng Blazor Server Static SSR và tương tác Form native. |
| **2** | Xác thực người dùng bằng Cookie Authentication | `src/ITCareerPlatform.Web/Program.cs` (L30-50), `Services.cs` (`AuthService`) | **VERIFIED** | Dùng `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)` và `SecurityStamp`. |
| **3** | Phân quyền 3 vai trò: Admin, Mentor, SinhVienIT | `Models.cs` (`Roles`), `AppDbContext.cs` (Seed 3 role), `UserList.razor` | **VERIFIED** | Đã kiểm chứng phân quyền endpoint và component qua `[Authorize(Roles = ...)]`. |
| **4** | Tích hợp Trí tuệ nhân tạo Google Gemini API | `Services/AiService.cs` (`GeminiAiService`), `appsettings.json` | **VERIFIED** | Endpoint gọi Gemini 1.5/2.0 REST API để sinh % phù hợp, điểm mạnh, thiếu sót, lộ trình học. |
| **5** | Cơ chế Offline Heuristic Fallback khi mất mạng/hết quota | `Services/AiService.cs` (`HeuristicAiService`), `AiServiceTests.cs` | **VERIFIED** | Thuật toán đối sánh TechStack tất định (deterministic), luôn trả về điểm trong [0, 100]. |
| **6** | Quét an toàn tệp CV chặn file thực thi & virus EICAR | `Services/CvUtilities.cs` (`CvScanner`), `ProfileServiceTests.cs` | **VERIFIED** | Chặn magic bytes `4D 5A` (MZ), `7F 45 4C 46` (ELF) và chuỗi chữ ký EICAR chuẩn. Giới hạn 5MB. |
| **7** | Đóng băng bản chụp CV vào đơn ứng tuyển | `Models.cs` (`Application.CvDataSnapshot`), `ApplicationServiceTests.cs` | **VERIFIED** | Đơn lưu trữ bản chụp độc lập, sinh viên đổi CV sau này không ảnh hưởng đơn đã nộp. |
| **8** | Quản lý trạng thái đơn theo máy trạng thái nghiêm ngặt | `Models.cs` (`ApplicationStatusFlow`), `StatusFlowTests.cs` | **VERIFIED** | 5 trạng thái chính, chặn chuyển trạng thái phi lý, lưu lịch sử vào `ApplicationStatusHistories`. |
| **9** | Hàng đợi email Outbox và tệp đính kèm lịch `.ics` | `Models.cs` (`EmailOutbox`), `OutboxSender.cs`, `EmailOutboxTests.cs` | **VERIFIED** | Tiến trình nền quét hàng đợi 30s/lần, sinh tệp `interview.ics` chuẩn RFC 5545. |
| **10** | Chuẩn hóa thời gian UTC và chuyển đổi múi giờ Việt Nam | `UiHelpers.cs` (`Ui.ToVietnamTime`), `VietnamTimeTests.cs` | **VERIFIED** | Mọi mốc thời gian lưu UTC trong CSDL, chỉ hiển thị giờ VN (UTC+7) trên UI. |
| **11** | Tuân thủ bảo vệ dữ liệu cá nhân (Nghị định 13/2023/NĐ-CP) | `Models.cs` (`AiConsentAt`, `AiConsentVersion`), `AiConsentTests.cs` | **VERIFIED** | Bắt buộc người dùng đồng ý trước khi gửi CV cho Gemini, cho phép rút lại sự đồng ý. |
| **12** | Tổng số bài kiểm thử tự động xUnit đạt 415 tests | `BAO_CAO_TRIEN_KHAI_UI_ATS_LAN_2.md`, `tests/ITCareerPlatform.Tests/` | **VERIFIED** | 415/415 tests PASSED (100% xanh) trên nền tảng .NET 10 xUnit + SQLite In-Memory. |
| **13** | Đóng gói triển khai bằng Docker Compose đa dịch vụ | `Dockerfile`, `docker-compose.yml` | **VERIFIED** | Đóng gói Web App .NET 10 kết nối SQL Server 2022 qua mạng nội bộ `itcp_net`. |
| **14** | Dự án có 5 thành viên thuộc Nhóm 9 môn QLDA CNTT | `README.md` (Dòng 3 ghi nhận Nhóm 9) | **NEED EVIDENCE** | Git chỉ ghi nhận 3 tác giả commit; cần bổ sung danh tính 2 thành viên còn lại từ biên bản lớp. |
| **15** | Đề xuất 68 User Stories ban đầu trong Product Backlog | Tài liệu môn học / Yêu cầu đề bài | **NEED EVIDENCE** | Trong repo có 18 stories ATS và các gói P0/P1/P2/UI; cần tệp xuất Jira để minh chứng danh sách 68 stories. |
| **16** | Ước lượng độ phức tạp bằng Planning Poker (Fibonacci) | Phương pháp quản lý Scrum theo đề bài | **NEED EVIDENCE** | Nhóm áp dụng phân rã độ phức tạp nhưng số điểm Story Point cụ thể từng User Story cần xuất từ Jira. |
| **17** | Chưa cài đặt Rate Limit cho tính năng AI Evaluation | `BAO_CAO_KIEM_THU.md` (Test Case A5), `Program.cs` | **VERIFIED** | Đã xác nhận không có `RateLimiter` middleware trong mã nguồn, ghi nhận trung thực vào báo cáo. |
| **18** | Chức năng gửi email SMTP thật chưa cấu hình mật khẩu | `BAO_CAO_TRIEN_KHAI_UI_ATS_LAN_2.md` (CONFIG-1) | **VERIFIED** | Chưa có file `appsettings.Development.json` thật, hệ thống hoạt động với fallback hiển thị mật khẩu tạm. |

---

## KẾT LUẬN KIỂM TOÁN
- **Số khẳng định đã xác minh (VERIFIED):** 15/18 (83.3%)
- **Số khẳng định cần bổ sung bằng chứng Jira/Biên bản (NEED EVIDENCE):** 3/18 (16.7%)
- **Số khẳng định bịa đặt / sai sự thật (UNVERIFIED):** 0/18 (0.0%)
- **Đánh giá:** Toàn bộ nội dung báo cáo bám sát thực tế phát triển phần mềm của repository, thể hiện tính trung thực khoa học cao nhất.
