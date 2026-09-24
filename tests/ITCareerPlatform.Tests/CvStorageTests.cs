using System.Text;
using ITCareerPlatform.Models;
using ITCareerPlatform.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ITCareerPlatform.Tests;

/// <summary>
/// P2-2: nội dung CV ra blob storage, CSDL chỉ giữ khóa.
///
/// Vấn đề cũ: Application.CvDataSnapshot lưu MỘT BẢN CV (tới 5MB) cho MỖI đơn, cộng bản gốc
/// trong CandidateProfile.CvData. Một sinh viên nộp 20 tin là 21 bản CV trong CSDL.
/// Ý tưởng "đóng băng CV tại thời điểm nộp" được GIỮ NGUYÊN; chỉ chỗ lưu đổi.
/// </summary>
public class CvStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "itcp-cv-unit", Guid.NewGuid().ToString("N"));
    private DiskCvStorage NewStorage() => new(_root);

    public void Dispose()
    {
        try { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
        catch (IOException) { }
    }

    private static byte[] Pdf(string body) => Encoding.UTF8.GetBytes("%PDF-1.4 " + body);

    // ===== Khử trùng lặp =====

    /// <summary>
    /// Hai CV có nội dung GIỐNG NHAU cho ra cùng một khóa, và chỉ tốn một tệp trên đĩa. Đây
    /// chính là chỗ tiết kiệm lớn nhất: bản chụp lúc nộp thường giống hệt CV gốc.
    /// </summary>
    [Fact]
    public async Task IdenticalContent_ProducesTheSameKey_AndOneFile()
    {
        var storage = NewStorage();
        var data = Pdf("noi dung cv");

        var k1 = await storage.SaveAsync(data, "cv.pdf");
        var k2 = await storage.SaveAsync(data, "cv.pdf");

        Assert.Equal(k1, k2);
        Assert.Single(Directory.GetFiles(_root, "*.pdf", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task DifferentContent_ProducesDifferentKeys()
    {
        var storage = NewStorage();

        var k1 = await storage.SaveAsync(Pdf("cv cua A"), "cv.pdf");
        var k2 = await storage.SaveAsync(Pdf("cv cua B"), "cv.pdf");

        Assert.NotEqual(k1, k2);
    }

    /// <summary>
    /// Khóa dựng từ HASH NỘI DUNG, không từ tên tệp người dùng gửi lên. Ghép tên tệp vào
    /// đường dẫn là mở sẵn một lỗ ghi tệp tùy ý: "../../appsettings.json" sẽ ghi ra ngoài
    /// thư mục lưu trữ.
    /// </summary>
    [Fact]
    public async Task Key_DoesNotContainOrDependOnTheUserFileName()
    {
        var storage = NewStorage();
        var data = Pdf("cung mot noi dung");

        var normal = await storage.SaveAsync(data, "cv.pdf");
        var nasty = await storage.SaveAsync(data, "../../appsettings.json");

        Assert.Equal(normal, nasty);                       // tên tệp không ảnh hưởng tới khóa
        Assert.DoesNotContain("appsettings", normal);
        Assert.DoesNotContain("..", normal);
        Assert.DoesNotContain("/", normal);
        Assert.DoesNotContain("\\", normal);
    }

    [Fact]
    public async Task SavedContent_ReadsBackByteForByte()
    {
        var storage = NewStorage();
        var data = Pdf("noi dung cv co dau tieng Viet: Nguyễn Văn A");

        var key = await storage.SaveAsync(data, "cv.pdf");

        Assert.Equal(data, await storage.ReadAsync(key));
    }

    [Fact]
    public async Task Delete_RemovesTheContent()
    {
        var storage = NewStorage();
        var key = await storage.SaveAsync(Pdf("cv"), "cv.pdf");

        await storage.DeleteAsync(key);

        Assert.Null(await storage.ReadAsync(key));
    }

    // ===== Khóa không hợp lệ =====

    /// <summary>
    /// Khóa được kiểm lại ở đường ĐỌC, không chỉ tin vào đường ghi: giá trị trong CSDL có
    /// thể đã bị sửa tay, và một khóa như "../appsettings.json" mà lọt qua sẽ đọc được tệp
    /// bất kỳ trên máy chủ.
    /// </summary>
    [Theory]
    [InlineData("../../appsettings.json")]
    [InlineData("..\\..\\web.config")]
    [InlineData("/etc/passwd")]
    [InlineData("khong-phai-hash.pdf")]
    [InlineData("")]
    public async Task InvalidKey_ReadsNothing(string key)
    {
        Assert.Null(await NewStorage().ReadAsync(key));
    }

    [Fact]
    public async Task ReadingAnUnknownButWellFormedKey_ReturnsNull()
    {
        var key = new string('a', 64) + ".pdf";

        Assert.Null(await NewStorage().ReadAsync(key));
    }

    [Fact]
    public void ComputeKey_UsesDocxExtension_ForWordFiles()
    {
        var data = Pdf("x");

        Assert.EndsWith(".docx", DiskCvStorage.ComputeKey(data, "ho-so.DOCX"));
        Assert.EndsWith(".pdf", DiskCvStorage.ComputeKey(data, "ho-so.pdf"));
        Assert.EndsWith(".pdf", DiskCvStorage.ComputeKey(data, null));
    }

    // ===== Đường ghi và đường đọc của ứng dụng =====

    [Fact]
    public async Task SaveCv_PutsContentInStorage_AndClearsTheLegacyColumn()
    {
        using var t = new TestDb();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        var svc = new ProfileService(t.Db, t.CvStorage);

        var (ok, err) = await svc.SaveCvAsync(sv.Id, Pdf("noi dung cv"), "cv.pdf", "application/pdf", aiConsentGiven: true);

        Assert.True(ok, err);
        var p = t.NewContext().CandidateProfiles.Single(x => x.UserId == sv.Id);
        Assert.NotNull(p.CvStorageKey);
        Assert.Null(p.CvData);          // không lưu hai bản
        Assert.True(p.HasCv);
    }

    /// <summary>Đơn cũ chỉ có byte[] phải vẫn đọc được — đó là lý do cột cũ được giữ lại.</summary>
    [Fact]
    public async Task LegacyApplication_WithOnlyBytes_IsStillReadable()
    {
        using var t = new TestDb();
        var m = t.AddMentor();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        var p = t.AddProfile(sv.Id);
        var job = t.AddJob(m.Id);
        var app = t.Db.Applications.Add(new Application
        {
            JobId = job.Id, CandidateProfileId = p.Id,
            CvFileNameSnapshot = "cu.pdf",
            CvDataSnapshot = Pdf("CV CU"),          // chỉ có cột byte[], không có khóa
            CvContentTypeSnapshot = "application/pdf",
            Status = ApplicationStatus.Submitted
        }).Entity;
        t.Db.SaveChanges();

        var svc = new ApplicationService(t.Db, new NotificationService(t.Db), t.CvStorage);
        var cv = await svc.ReadCvAsync(app.Id);

        Assert.NotNull(cv);
        Assert.Contains("CV CU", Encoding.UTF8.GetString(cv!.Value.Data));
        Assert.Equal("cu.pdf", cv.Value.FileName);
    }

    /// <summary>Nộp đơn chỉ chép KHÓA, không nhân thêm 5MB nội dung cho mỗi đơn.</summary>
    [Fact]
    public async Task Apply_CopiesTheKey_NotTheBytes()
    {
        using var t = new TestDb();
        var m = t.AddMentor();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        await new ProfileService(t.Db, t.CvStorage).SaveCvAsync(sv.Id, Pdf("cv"), "cv.pdf", "application/pdf", aiConsentGiven: true);
        var job = t.AddJob(m.Id);
        var svc = new ApplicationService(t.Db, new NotificationService(t.Db), t.CvStorage);

        Assert.True(svc.Apply(job.Id, sv.Id, out var msg), msg);

        var a = t.NewContext().Applications.Single();
        Assert.NotNull(a.CvStorageKeySnapshot);
        Assert.Null(a.CvDataSnapshot);
        Assert.True(a.HasCvSnapshot);
    }

    // ===== Di trú dữ liệu =====

    private static CvMigrationRunner NewRunner(TestDb t) =>
        new(t.Db, t.CvStorage, NullLogger<CvMigrationRunner>.Instance);

    [Fact]
    public async Task Migration_FillsKeysForLegacyRows()
    {
        using var t = new TestDb();
        var m = t.AddMentor();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        var p = t.AddProfile(sv.Id);                    // AddProfile chỉ đặt CvData
        var job = t.AddJob(m.Id);
        t.Db.Applications.Add(new Application
        {
            JobId = job.Id, CandidateProfileId = p.Id,
            CvFileNameSnapshot = "cv.pdf", CvDataSnapshot = Pdf("CV"),
            Status = ApplicationStatus.Submitted
        });
        t.Db.SaveChanges();

        var result = await NewRunner(t).RunAsync();

        Assert.Equal(1, result.Profiles);
        Assert.Equal(1, result.Applications);
        using var v = t.NewContext();
        Assert.NotNull(v.CandidateProfiles.Single().CvStorageKey);
        Assert.NotNull(v.Applications.Single().CvStorageKeySnapshot);
    }

    /// <summary>
    /// CHẠY LẠI KHÔNG TẠO BẢN THỨ HAI. Một lần di trú trên CSDL thật có thể bị ngắt giữa
    /// chừng, và lúc đó thứ duy nhất làm được là chạy lại từ đầu.
    /// </summary>
    [Fact]
    public async Task Migration_IsIdempotent()
    {
        using var t = new TestDb();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        t.AddProfile(sv.Id);
        var runner = NewRunner(t);
        var first = await runner.RunAsync();

        var second = await runner.RunAsync();

        Assert.Equal(1, first.Profiles);
        Assert.True(second.DidNothing);
        Assert.Single(Directory.GetFiles(t.CvRoot, "*.pdf", SearchOption.AllDirectories));
    }

    /// <summary>Di trú KHÔNG xóa cột byte[] — giữ bản gốc là cách duy nhất còn quay lui được.</summary>
    [Fact]
    public async Task Migration_KeepsTheLegacyColumn()
    {
        using var t = new TestDb();
        var sv = t.AddUser("SV", "sv@itcp.vn", Roles.StudentId);
        t.AddProfile(sv.Id);

        await NewRunner(t).RunAsync();

        var p = t.NewContext().CandidateProfiles.Single();
        Assert.NotNull(p.CvStorageKey);
        Assert.NotNull(p.CvData);
    }

    /// <summary>
    /// Upload hỏng hoặc di trú chạy dở để lại byte[0] trong cột cũ. Trả nó về như một CV hợp
    /// lệ nghĩa là người dùng tải được tệp 0 byte, còn AI chấm trên một CV rỗng mà không có
    /// lỗi nào báo. Cả hai đường đọc phải nói CÙNG một điều: không có CV.
    /// </summary>
    [Fact]
    public async Task ZeroLengthLegacyColumn_IsTreatedAsNoCv_OnEveryReadPath()
    {
        using var t = new TestDb();
        var job = t.AddJob(t.AddMentor().Id);
        var profile = t.AddProfile(t.AddUser("SV", "sv-empty@itcp.vn", Roles.StudentId).Id, withCv: false);
        profile.CvData = Array.Empty<byte>();
        profile.CvStorageKey = null;
        t.Db.SaveChanges();

        var app = t.AddApplication(job.Id, profile.Id);
        app.CvDataSnapshot = Array.Empty<byte>();
        app.CvStorageKeySnapshot = null;
        t.Db.SaveChanges();

        // 1) đường tải CV của đơn
        var apps = new ApplicationService(t.Db, new NotificationService(t.Db), t.CvStorage, new AuditService(t.Db));
        Assert.Null(await apps.ReadCvAsync(app.Id));

        // 2) đường đọc CV của hồ sơ (tự kiểm tra, chấm điểm)
        Assert.Null(await new ProfileService(t.Db, t.CvStorage).ReadCvAsync(profile));

        // Cột "CV" trong danh sách ứng viên vẫn đánh dấu là CÓ: nó được tính bằng một vị ngữ
        // SQL (blob IS NOT NULL) và EF không dịch được độ dài của byte[] sang SQL. Chấp nhận
        // sai lệch đó cho một hàng dữ liệu hỏng, đổi lấy việc danh sách không phải nạp nội
        // dung CV về chỉ để đếm byte.
    }

    /// <summary>
    /// Và vẫn phải đúng SAU KHI di trú. CvMigrationRunner lọc theo "CvData != null", mà byte[0]
    /// không phải null — nên nó ghi một tệp 0 byte vào storage và gán cho hồ sơ một khóa hợp lệ.
    /// Nếu nhánh đọc từ storage chỉ kiểm khác-null thì CV rỗng lại được trả về như CV thật, tức
    /// là lỗi quay lại đúng chỗ vừa vá — chỉ khác đường đi.
    /// </summary>
    [Fact]
    public async Task AfterMigration_AnEmptyCvIsStillNoCv_EvenThoughItNowHasAStorageKey()
    {
        using var t = new TestDb();
        var profile = t.AddProfile(t.AddUser("SV", "sv-empty@itcp.vn", Roles.StudentId).Id, withCv: false);
        profile.CvData = Array.Empty<byte>();
        profile.CvStorageKey = null;
        t.Db.SaveChanges();

        var migrated = await NewRunner(t).RunAsync();
        Assert.Equal(1, migrated.Profiles);   // hàng rỗng vẫn được xử lý một lần

        var after = t.NewContext().CandidateProfiles.Single();
        // KHÔNG nhận khóa: blob rỗng chẳng có gì để chuyển, và một khóa hợp lệ ở đây sẽ làm cột
        // "CV" trong danh sách ứng viên báo "có CV" cho một hồ sơ không có CV.
        Assert.Null(after.CvStorageKey);
        Assert.Null(after.CvData);            // cột rỗng dọn luôn, nên hàng rời khỏi điều kiện lọc
        Assert.False(after.HasCv);

        // Và đọc ra vẫn là "không có CV".
        Assert.Null(await new ProfileService(t.Db, t.CvStorage).ReadCvAsync(after));

        // Chạy lại lần hai: không còn gì để làm (idempotent, và không có vòng lặp vô hạn).
        Assert.True((await NewRunner(t).RunAsync()).DidNothing);
    }
}
