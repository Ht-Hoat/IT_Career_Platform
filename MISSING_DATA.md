# DANH SÁCH DỮ LIỆU CẦN BỔ SUNG MINH CHỨNG (MISSING DATA)
**Dự án:** IT Career Platform – Hệ thống nền tảng nghề nghiệp IT tích hợp ATS và AI  
**Học phần:** Quản lý Dự án Công nghệ Thông tin (Nhóm 9)  
**Nguyên tắc:** Tuân thủ quy tắc chống bịa dữ liệu — Các thông tin chưa thể kiểm chứng 100% bằng mã nguồn, Git log hoặc tài liệu markdown nội bộ đều phải được đánh dấu minh bạch và chỉ rõ nguồn cần thu thập.

---

## BẢNG DANH MỤC DỮ LIỆU THIẾU

| STT | Dữ liệu còn thiếu / Chưa xác minh | Hiện trạng trong Repository | Cần lấy từ đâu / Hành động bổ sung | Mục báo cáo liên quan |
| :---: | :--- | :--- | :--- | :--- |
| **1** | **Danh tính 2 thành viên còn lại (Thành viên 4 & 5)** | Git log và commit chỉ ghi nhận 3 tác giả: `Ht-Hoat`, `Nguyễn Viết Hùng`, `Võ Nguyên Anh Kiệt`. Chưa có commit hay email của 2 thành viên còn lại. | Danh sách phân công nhóm của lớp, biên bản họp nhóm nội bộ hoặc bảng điểm quá trình. | Mục 1.2 (W5HH - Who), Mục 2.2 (Scrum Roles) |
| **2** | **Scrum Master chính thức của nhóm** | 3 thành viên trên Git đóng vai trò Product Owner, Core Backend Dev, Frontend/QA Dev. Chưa có văn bản chỉ định Scrum Master. | Biên bản bầu chọn nhóm hoặc file Jira Project Settings. | Mục 2.2 (Tổ chức nhóm theo Scrum Roles) |
| **3** | **Danh sách đầy đủ 68 User Stories ban đầu** | Trong mã nguồn và tài liệu nội bộ ghi nhận cụ thể 18 User Stories cốt lõi (ATS-01..18) kèm các gói tính năng mở rộng (P0, P1, P2, N1, N2, UI-1..13). Con số 68 User Stories ban đầu chưa có bảng liệt kê chi tiết trong git. | Tệp xuất dữ liệu Jira (`Jira-Export.csv`/`.xlsx`), Product Backlog thô ban đầu của nhóm. | Mục 1.2 (W5HH - How much), Mục 3.1 (Phân loại MoSCoW), Mục 3.3 (Product Backlog) |
| **4** | **Số điểm Story Point cụ thể từng User Story** | Nhóm áp dụng phân rã tính năng theo task và độ phức tạp, chưa lưu trữ giá trị Fibonacci Story Point cho từng User Story trong Git commit. | Bảng Jira Backlog, Board Sprint Planning, phiếu Planning Poker của nhóm. | Mục 3.2 (Ước lượng khối lượng công việc), Mục 3.3 (Bảng Product Backlog) |
| **5** | **Thời gian bắt đầu / kết thúc chính thức của Sprint 1, 2, 3 trên Jira** | Git commit ghi nhận commit v2 tích hợp ngày 05/09/2026, các đợt phát triển tiếp diễn đến 23/09/2026. Chưa có mốc thời gian sprint planning / sprint review từng đợt trên Jira. | Lịch sử Sprint trên Jira Software (Sprint Start Date, Sprint End Date). | Mục 1.2 (W5HH - When), Mục 2.3 (Quy trình thực thi Scrum), Mục 4 (Kết quả thực thi Sprint) |
| **6** | **Tài liệu kiểm thử UAT có chữ ký nghiệm thu** | Có báo cáo kiểm thử tự động xUnit (415 test pass) và tài liệu hướng dẫn test tay (`HUONG_DAN_TEST.md`), nhưng chưa có biên bản ký nghiệm thu UAT người dùng cuối. | Biên bản nghiệm thu dự án giữa nhóm sinh viên và Giảng viên / Người dùng thử nghiệm. | Mục 4.4 (Sprint 4 - UAT), Mục 6.4 (Đánh giá Increment) |
| **7** | **Chi phí tài chính và ngân sách thực tế của dự án sinh viên** | Dự án học thuật không phát sinh ngân sách tài chính thương mại (sử dụng gói miễn phí của Google Gemini AI Studio, SQL Server Express, Docker Community, GitHub). Có tài liệu tham khảo bài toán Function Point HUCE trong repo nhưng là ví dụ mẫu. | Bảng kê khai chi phí hạ tầng (nếu có thuê VPS/Domain) hoặc xác nhận dự án sử dụng hạ tầng nghiên cứu học thuật 0 VNĐ. | Mục 1.2 (W5HH - How much), Chương 2 (Project Resources) |

---

## HƯỚNG DẪN XỬ LÝ TRONG BÁO CÁO LATEX
- Tại các vị trí thiếu dữ liệu trong báo cáo, hệ thống sẽ chèn nhãn học thuật rõ ràng:
  `\textbf{[CHƯA CÓ DỮ LIỆU XÁC MINH - CẦN BỔ SUNG TỪ JIRA/BIÊN BẢN]}`
- Tuyệt đối không tự suy đoán, bịa đặt số liệu hay tên người nhằm làm đẹp báo cáo.
