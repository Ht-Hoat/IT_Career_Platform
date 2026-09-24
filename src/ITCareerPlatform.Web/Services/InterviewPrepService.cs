using ITCareerPlatform.Data;
using ITCareerPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace ITCareerPlatform.Services;

/// <summary>
/// Mọi thứ trang đơn của sinh viên cần về phần luyện phỏng vấn.
/// <para><see cref="Questions"/> là bộ MỚI NHẤT, đang hiển thị đầy đủ ở đầu mục.</para>
/// <para><see cref="Previous"/> là những bộ TRƯỚC ĐÓ — đã bỏ bộ mới nhất ra, vì nó nằm ngay
/// bên trên: để nguyên cả danh sách thì bộ vừa tạo hiện hai lần trên cùng một trang.</para>
/// </summary>
public record InterviewPrepState(
    InterviewQuestionSet? Questions,
    IReadOnlyList<InterviewQuestionSet> Previous,
    string? BlockedReason);

/// <summary>
/// Bộ câu hỏi LUYỆN PHỎNG VẤN cho sinh viên: khi được mời phỏng vấn, sinh viên tạo một bộ câu
/// hỏi nhà tuyển dụng nhiều khả năng sẽ hỏi (dựa trên CV đã nộp và JD), kèm gợi ý cách trả lời.
///
/// Trước đây bộ câu hỏi được sinh cho phía nhà tuyển dụng. Nó chuyển sang đây vì người cần
/// chuẩn bị là ứng viên; nhà tuyển dụng tự biết mình muốn hỏi gì.
/// </summary>
public interface IInterviewPrepService
{
    /// <summary>
    /// Trạng thái đầy đủ cho trang đơn, trong HAI truy vấn (đơn + lịch sử). Null nếu đơn không
    /// tồn tại hoặc không thuộc sinh viên này — hỏi từng mảnh sẽ thành 5 lượt đi CSDL vì mỗi
    /// hàm phải tự kiểm lại quyền sở hữu.
    /// </summary>
    InterviewPrepState? GetState(int appId, int candidateUserId);

    /// <summary>Lý do chưa tạo được lúc này (để giao diện giải thích thay vì hiện nút), hoặc null nếu tạo được.</summary>
    string? WhyNot(int appId, int candidateUserId);

    Task<(bool ok, string message)> GenerateAsync(int appId, int candidateUserId, CancellationToken ct = default);
}

public class InterviewPrepService(
    AppDbContext db,
    IApplicationService applications,
    IAiService ai,
    IAiInputBuilder inputBuilder,
    TimeProvider? clock = null) : IInterviewPrepService
{
    /// <summary>
    /// Tạo lại tối đa một lần mỗi chừng này giờ. Mỗi lần tạo là một lần gọi Gemini (quota miễn
    /// phí có trần); không có giới hạn thì một người bấm liên tục là cả hệ thống mất tính năng.
    /// </summary>
    public const int RegenerateCooldownHours = 24;

    // Đơn của người khác và đơn không tồn tại trả về CÙNG một câu — không xác nhận id nào có thật.
    private const string NotFound = "Không tìm thấy đơn.";

    public InterviewPrepState? GetState(int appId, int candidateUserId)
    {
        var row = ReadRow(appId, candidateUserId);
        if (row is null) return null;

        var current = ApplicationService.ParseQuestions(row.Questions, row.Source);

        // Bỏ bản chụp mới nhất NGAY TRONG SQL khi nó chính là bộ đang hiện ở trên: kéo nó về
        // rồi Skip(1) ở C# nghĩa là mỗi lần mở trang truyền thừa cả một bộ câu hỏi qua dây
        // và parse JSON của nó hai lần. Đơn tạo trước khi có bảng bản chụp không có dòng nào
        // để bỏ, nên chỉ bỏ khi thực sự đọc được bộ hiện tại từ cột.
        var previous = applications.GetAiQuestionHistory(appId, skip: current is null ? 0 : 1);

        return new InterviewPrepState(current, previous, BlockedReason(row));
    }

    // KHÔNG đi qua GetState: câu trả lời là một chuỗi, không cần tới lịch sử. GenerateAsync
    // gọi hàm này trước mỗi lần tạo, nên nối nó vào GetState là bắt mỗi lần bấm "Tạo bộ câu
    // hỏi mới" phải tải về toàn bộ các bộ cũ rồi vứt đi.
    public string? WhyNot(int appId, int candidateUserId) =>
        ReadRow(appId, candidateUserId) is { } row ? BlockedReason(row) : NotFound;

    /// <summary>Hàng đơn mà cả hai đường đều cần. Chủ đơn lọc ngay trong SQL: đơn của người
    /// khác trả null y hệt đơn không tồn tại.</summary>
    private Row? ReadRow(int appId, int candidateUserId) =>
        db.Applications.AsNoTracking()
            .Where(x => x.Id == appId && x.CandidateProfile!.UserId == candidateUserId)
            .Select(x => new Row(x.Status, x.CandidateProfile!.AiConsentAt != null,
                                 x.AiQuestionsAt, x.AiQuestions, x.AiQuestionsSource))
            .FirstOrDefault();

    private sealed record Row(string Status, bool Consented, DateTime? AiQuestionsAt, string? Questions, string? Source);

    private string? BlockedReason(Row row)
    {
        if (row.Status != ApplicationStatus.Interview)
            return "Bộ câu hỏi luyện tập mở khi bạn được mời phỏng vấn.";
        // Bộ câu hỏi soạn từ nội dung CV gửi tới dịch vụ AI — cùng luật đồng ý với chấm điểm.
        if (!row.Consented) return AiConsentGate.BlockedForStudent;

        if (row.AiQuestionsAt is DateTime last)
        {
            var nextAllowed = last.AddHours(RegenerateCooldownHours);
            if (VietnamDateHelper.UtcNow(clock) < nextAllowed)
                return $"Bạn đã tạo bộ câu hỏi lúc {Ui.DateTimeText(last)}. " +
                       $"Có thể tạo lại sau {Ui.DateTimeText(nextAllowed)}.";
        }
        return null;
    }

    public async Task<(bool ok, string message)> GenerateAsync(int appId, int candidateUserId, CancellationToken ct = default)
    {
        var reason = WhyNot(appId, candidateUserId);
        if (reason is not null) return (false, reason);

        // Bản đầy đủ (kèm CV đã nộp) — AiInputBuilder cần nội dung CV để soạn câu hỏi bám hồ sơ.
        var a = applications.GetById(appId);
        if (a?.CandidateProfile is null || a.Job is null) return (false, NotFound);

        // Lần gọi mô hình nằm NGOÀI mọi giao dịch CSDL, giống SelfCheckService.
        var set = await ai.GenerateQuestionsAsync(await inputBuilder.ForApplicationAsync(a, a.CandidateProfile, ct), ct);

        // Hỏi CHÍNH lần ghi xem nó có ghi được không. Đọc lại bộ câu hỏi để kết luận sẽ thấy
        // bộ CŨ còn nguyên đó và báo "đã tạo xong" cho một lần tạo chẳng lưu được gì.
        return applications.SaveAiQuestions(appId, set)
            ? (true, "Đã tạo bộ câu hỏi luyện phỏng vấn.")
            : (false, "Không tạo được bộ câu hỏi, vui lòng thử lại sau.");
    }
}
