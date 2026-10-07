using System.Net;
using Xunit;

namespace ITCareerPlatform.Tests;

/// <summary>
/// Trang /jobs/{id}/detail trên app THẬT.
///
/// Trang này khai [Authorize(Roles = AdminOrMentor)] — câu đó chỉ nói "được vào trang", không
/// nói "được xem tin NÀY". Thiếu một bước kiểm quyền sở hữu trong OnInitializedAsync thì mọi
/// Mentor mở thẳng URL là đọc được JD, khoảng lương, hạn nộp và số ứng viên của đối thủ.
/// Test ở tầng HTTP chứ không phải tầng service, vì đúng chỗ từng thiếu là trang.
///
/// Lớp riêng (AppFactory riêng) vì giới hạn 10 lần đăng nhập/phút tính theo từng app.
/// </summary>
public class HttpJobDetailAccessTests(AppFactory app) : IClassFixture<AppFactory>
{
    /// <summary>Tin "DevOps Engineer" trong dữ liệu mẫu do mentor2 đăng, không phải mentor.</summary>
    private const string Mentor2Job = "DevOps Engineer";

    [Fact]
    public async Task OtherMentor_SeesNothingOfSomeoneElsesJob()
    {
        var jobId = app.JobId(Mentor2Job);
        var text = WebUtility.HtmlDecode(await (await app.LoginAs("mentor@itcp.vn")).GetStringAsync($"/jobs/{jobId}/detail"));

        // Đúng một trang "không tìm thấy" — cùng câu với tin không tồn tại, nên URL này
        // không xác nhận được là tin đó có thật hay không.
        Assert.Contains("Không tìm thấy tin tuyển dụng", text);

        // Và không một mẩu nội dung nào lọt ra.
        Assert.DoesNotContain("Vận hành cụm Kubernetes", text);      // JD
        Assert.DoesNotContain("Xem danh sách ứng viên", text);       // số ứng viên
        Assert.DoesNotContain("Trần Văn Tiền", text);                // tên người đăng
        Assert.DoesNotContain("30–45 tr", text);                     // khoảng lương
    }

    /// <summary>Tin không tồn tại trả về đúng trang mà tin của người khác trả về.</summary>
    [Fact]
    public async Task MissingJob_LooksExactlyLikeSomeoneElsesJob()
    {
        var text = WebUtility.HtmlDecode(await (await app.LoginAs("mentor@itcp.vn")).GetStringAsync("/jobs/999999/detail"));
        Assert.Contains("Không tìm thấy tin tuyển dụng", text);
    }

    [Fact]
    public async Task OwningMentor_SeesTheJob_AndCanEditIt()
    {
        var jobId = app.JobId(Mentor2Job);
        var html = await (await app.LoginAs("mentor2@itcp.vn")).GetStringAsync($"/jobs/{jobId}/detail");

        Assert.Contains("Vận hành cụm Kubernetes", WebUtility.HtmlDecode(html));
        Assert.Contains("Xem danh sách ứng viên", WebUtility.HtmlDecode(html));
        Assert.Contains($"/jobs/edit/{jobId}", html);
    }

    /// <summary>
    /// Trang SỬA cũng vậy: [Authorize(Roles = Mentor)] cho mọi Mentor vào, nên nếu hàm đọc
    /// không tự kiểm chủ sở hữu thì form sửa hiện nguyên JD, lương và công ty của tin người khác.
    /// </summary>
    [Fact]
    public async Task OtherMentor_CannotOpenTheEditFormOfSomeoneElsesJob()
    {
        var jobId = app.JobId(Mentor2Job);
        var html = await (await app.LoginAs("mentor@itcp.vn")).GetStringAsync($"/jobs/edit/{jobId}");
        var text = WebUtility.HtmlDecode(html);

        Assert.Contains("Không tìm thấy tin tuyển dụng", text);
        Assert.DoesNotContain("Vận hành cụm Kubernetes", text);   // JD
        Assert.DoesNotContain($"/jobs/{jobId}/update", html);     // không có form sửa nào
    }

    [Fact]
    public async Task OwningMentor_CanOpenTheEditForm()
    {
        var jobId = app.JobId(Mentor2Job);
        var html = await (await app.LoginAs("mentor2@itcp.vn")).GetStringAsync($"/jobs/edit/{jobId}");

        Assert.Contains("Vận hành cụm Kubernetes", WebUtility.HtmlDecode(html));
        Assert.Contains($"/jobs/{jobId}/update", html);
    }

    /// <summary>Admin giám sát: xem được mọi tin, nhưng không có nút sửa.</summary>
    [Fact]
    public async Task Admin_SeesTheJob_ButHasNoEditButton()
    {
        var jobId = app.JobId(Mentor2Job);
        var html = await (await app.LoginAs("admin@itcp.vn")).GetStringAsync($"/jobs/{jobId}/detail");

        Assert.Contains("Vận hành cụm Kubernetes", WebUtility.HtmlDecode(html));
        Assert.DoesNotContain($"/jobs/edit/{jobId}", html);
    }
}
