using System.Net;
using ITCareerPlatform.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ITCareerPlatform.Tests;

/// <summary>
/// Ba kết cục của một lần Admin đặt lại mật khẩu, trên app THẬT.
///
/// Cả ba giờ được mô tả bởi MỘT giá trị trong cookie one-shot chứ không phải một tổ hợp cờ
/// boolean trên URL. Hai nhánh có SMTP dưới đây trước không có test nào chạy qua: AppFactory
/// không cấu hình SMTP nên mọi lần reset đều rơi vào nhánh "hiện trên màn hình", và nhánh đó
/// đã có HttpEndpointTests lo.
/// </summary>
public class HttpResetPasswordOutcomeTests
{
    /// <summary>SMTP "đã cấu hình". <paramref name="fail"/> để diễn lại một lần gửi hỏng.</summary>
    private sealed class FakeEmail(bool fail) : IEmailSender
    {
        public bool IsConfigured => true;
        public Task SendAsync(EmailMessage message, CancellationToken ct = default) =>
            fail ? Task.FromException(new InvalidOperationException("SMTP hỏng")) : Task.CompletedTask;
    }

    /// <summary>
    /// Đặt lại mật khẩu của khoa@itcp.vn với một IEmailSender giả, rồi trả về trang /users đã
    /// render. WithWebHostBuilder dựng một host thứ hai nhưng dùng chung kết nối SQLite trong
    /// bộ nhớ của factory gốc, và SeedData là idempotent nên dữ liệu mẫu không bị gieo hai lần.
    /// </summary>
    private static async Task<string> ResetKhoaWithEmail(AppFactory app, bool mailFails)
    {
        var client = app.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<IEmailSender>();
            s.AddScoped<IEmailSender>(_ => new FakeEmail(mailFails));
        })).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var login = await client.PostAsync("/account/login", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["email"] = "admin@itcp.vn", ["password"] = AppFactory.SeedPassword }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var targetId = app.Query(db => db.Users.Single(u => u.Email == "khoa@itcp.vn").Id);
        var token = AppFactory.TokenIn(await client.GetStringAsync("/users"), "/users");

        var post = await client.PostAsync($"/users/{targetId}/reset-password", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));

        // MỌI kết cục về cùng một chỗ: URL không còn mang mô tả kết cục nào nữa.
        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/users?tempPw=1", post.Headers.Location?.OriginalString);

        return WebUtility.HtmlDecode(await client.GetStringAsync("/users?tempPw=1"));
    }

    [Fact]
    public async Task WhenEmailIsSent_ThePasswordIsNeverShownOnScreen()
    {
        using var app = new AppFactory();
        var page = await ResetKhoaWithEmail(app, mailFails: false);

        Assert.Contains("Đã gửi mật khẩu tạm thời thành công", page);
        Assert.Contains("khoa@itcp.vn", page);
        // Không có ô mật khẩu nào: gửi được rồi thì Admin không cần nhìn thấy nó.
        Assert.DoesNotContain("pw-display-box", page);

        // Và việc đặt lại đã thực sự xảy ra — mật khẩu mẫu không còn dùng được.
        var hash = app.Query(db => db.Users.Single(u => u.Email == "khoa@itcp.vn").PasswordHash);
        Assert.False(BCrypt.Net.BCrypt.Verify(AppFactory.SeedPassword, hash));
    }

    [Fact]
    public async Task WhenSendingFails_ThePasswordIsShownWithTheMailFailureWarning()
    {
        using var app = new AppFactory();
        var page = await ResetKhoaWithEmail(app, mailFails: true);

        // Gửi hỏng KHÔNG được làm hỏng việc đặt lại — mật khẩu đã đổi rồi, nên phải lùi về
        // hiện trên màn hình, nếu không thì tài khoản đó không ai vào được nữa.
        Assert.Contains("pw-display-box", page);
        Assert.Contains("gửi email thất bại", page);
        Assert.Contains("CHỈ hiển thị 1 lần", page);
    }
}
