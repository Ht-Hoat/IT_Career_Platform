using ITCareerPlatform.Data;
using ITCareerPlatform.Models;
using ITCareerPlatform.Services;
using Xunit;

namespace ITCareerPlatform.Tests;

/// <summary>
/// Hai thứ không có gì khác canh được: giới hạn cột của email xếp hàng, và cái mã migration
/// mà LegacySchemaBridge ghi tay vào bảng lịch sử.
///
/// Cả hai đều hỏng theo cách IM LẶNG ở máy phát triển (SQLite không có trần độ dài, và bridge
/// chỉ chạy trên SQL Server) rồi mới nổ trên CSDL thật.
/// </summary>
public class ColumnLimitAndMigrationGuardTests
{
    // ===== Giới hạn cột của email xếp hàng =====

    /// <summary>
    /// Tiêu đề email dựng từ "{Job.Title} — {Company.Name}", hai cột 160 ký tự. Cộng cả tiền
    /// tố là 354 ký tự trên một cột nvarchar(300). Và bản ghi này được Add TRONG cùng
    /// transaction với lần đổi trạng thái, nên một lần ghi hỏng ở đây làm nhà tuyển dụng
    /// KHÔNG đổi được trạng thái đơn — không phải chỉ mất một email.
    /// </summary>
    [Theory]
    [InlineData(ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected)]
    public void Compose_WithMaxLengthTitleAndCompany_StaysInsideEveryColumn(string status)
    {
        var app = new Application
        {
            Id = 1,
            Status = status,
            InterviewAt = DateTime.UtcNow.AddDays(1),
            InterviewNote = new string('N', 500),
            CandidateFeedback = new string('F', 1000),
            CandidateProfile = new CandidateProfile { FullName = new string('S', 120), Email = new string('e', 150) + "@x.vn" },
            Job = new Job
            {
                Title = new string('T', 160),
                Company = new Company { Name = new string('C', Company.NameLimit) }
            }
        };

        var mail = StatusEmailComposer.Compose(app, status, statusChanged: true);

        Assert.NotNull(mail);
        Assert.True(mail!.Subject.Length <= EmailOutbox.SubjectLimit,
            $"Subject dài {mail.Subject.Length} > {EmailOutbox.SubjectLimit}");
        Assert.True(mail.Body.Length <= EmailOutbox.BodyLimit,
            $"Body dài {mail.Body.Length} > {EmailOutbox.BodyLimit}");
        Assert.True(mail.ToEmail.Length <= EmailOutbox.ToEmailLimit,
            $"ToEmail dài {mail.ToEmail.Length} > {EmailOutbox.ToEmailLimit}");
    }

    /// <summary>Cắt rồi thì tiêu đề vẫn phải còn nhận ra được là email gì.</summary>
    [Fact]
    public void Compose_ClipsTheSubject_ButKeepsItsBeginning()
    {
        var app = new Application
        {
            Id = 1,
            Status = ApplicationStatus.Rejected,
            CandidateProfile = new CandidateProfile { FullName = "SV", Email = "sv@x.vn" },
            Job = new Job { Title = new string('T', 160), Company = new Company { Name = new string('C', 160) } }
        };

        var mail = StatusEmailComposer.Compose(app, ApplicationStatus.Rejected, statusChanged: true);

        Assert.StartsWith("Kết quả ứng tuyển: ", mail!.Subject);
        Assert.Equal(EmailOutbox.SubjectLimit, mail.Subject.Length);
    }

    // ===== Mã migration mà LegacySchemaBridge ghi tay =====

    /// <summary>
    /// Bridge chèn thẳng một dòng vào __EFMigrationsHistory với mã migration viết cứng. Nếu
    /// migration đầu tiên bị sinh lại hay đổi tên, mã đó trỏ vào một migration KHÔNG tồn tại:
    /// Migrate() sẽ chạy lại InitialCreate trên một CSDL đã có bảng, và app không khởi động
    /// được. Không có gì ở tầng biên dịch nối hai bên, nên test này là chỗ duy nhất phát hiện.
    /// </summary>
    [Fact]
    public void LegacySchemaBridge_MigrationId_MatchesTheRealFirstMigration()
    {
        var dir = MigrationsDir();
        var first = Directory.GetFiles(dir, "*.cs")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null && !n.EndsWith(".Designer") && n != "AppDbContextModelSnapshot")
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .First();

        Assert.Equal(LegacySchemaBridge.InitialMigrationId, first);
    }

    /// <summary>
    /// Script chạy nguyên khối trên CSDL thật, nên hai điều kiện an toàn phải còn nguyên:
    /// chạy trong MỘT giao dịch, và tự hủy giao dịch khi có lỗi giữa chừng.
    /// </summary>
    [Fact]
    public void LegacySchemaBridge_Script_IsOneAbortableTransaction()
    {
        var script = LegacySchemaBridge.Script;

        Assert.Contains("SET XACT_ABORT ON", script);
        Assert.Equal(1, Occurrences(script, "BEGIN TRANSACTION"));
        Assert.Equal(1, Occurrences(script, "COMMIT TRANSACTION"));
        // Và mã migration đi vào câu INSERT là chính hằng đã kiểm ở test trên.
        Assert.Contains(LegacySchemaBridge.InitialMigrationId, script);
    }

    private static int Occurrences(string haystack, string needle)
    {
        var n = 0;
        for (var i = haystack.IndexOf(needle, StringComparison.Ordinal); i >= 0;
             i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal)) n++;
        return n;
    }

    private static string MigrationsDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ITCareerPlatform.Web", "Migrations")))
            dir = dir.Parent;
        Assert.True(dir is not null, "Không tìm thấy thư mục Migrations.");
        return Path.Combine(dir!.FullName, "src", "ITCareerPlatform.Web", "Migrations");
    }
}
