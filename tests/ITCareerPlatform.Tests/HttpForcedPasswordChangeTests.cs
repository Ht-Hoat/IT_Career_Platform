using System.Net;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ITCareerPlatform.Tests;

/// <summary>
/// P0-3: người vừa bị Admin đặt lại mật khẩu phải bị giữ ở trang Đổi mật khẩu cho tới khi
/// đổi xong — nếu không, mật khẩu tạm mà Admin đọc qua điện thoại trở thành mật khẩu thật.
///
/// Danh sách đường ĐI ĐƯỢC giờ đọc từ metadata của route ([AllowWithExpiredPassword]) chứ
/// không từ một mảng đường dẫn trong middleware. Test này khóa cả hai vế: mọi đường khác bị
/// chặn, và ba đường thoát vẫn mở — quên một đường thoát nghĩa là khóa người dùng trong một
/// vòng chuyển hướng không lối ra, đúng thứ không được phép phát hiện ở môi trường thật.
///
/// Lớp riêng (AppFactory riêng) vì giới hạn 10 lần đăng nhập/phút tính theo từng app.
/// </summary>
public class HttpForcedPasswordChangeTests(AppFactory app) : IClassFixture<AppFactory>
{
    private const string Email = "khoa@itcp.vn";

    /// <summary>Bật/tắt cờ "buộc đổi mật khẩu" thẳng trong CSDL, như sau một lần Admin reset.</summary>
    private void SetForced(bool forced) =>
        app.Query(db => db.Users.Where(u => u.Email == Email)
                                .ExecuteUpdate(s => s.SetProperty(u => u.MustChangePassword, forced)));

    [Fact]
    public async Task ForcedUser_IsRedirected_AwayFromEveryOtherPage_ButNotFromTheEscapeRoutes()
    {
        var client = await app.LoginAs(Email);
        SetForced(true);
        try
        {
            // 1. Mọi trang khác đều bị đá về trang đổi mật khẩu.
            foreach (var blocked in new[] { "/", "/positions", "/my-applications", "/profile" })
            {
                var res = await client.GetAsync(blocked);
                Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
                Assert.Equal("/change-password?forced=1", res.Headers.Location?.OriginalString);
            }

            // 2. Trang đổi mật khẩu thì mở — nếu nó cũng bị chặn thì không còn lối ra nào.
            var page = await client.GetAsync("/change-password?forced=1");
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);

            // 3. Đăng xuất cũng phải mở: người không nhớ nổi mật khẩu tạm vẫn phải thoát được.
            var logout = await client.PostAsync("/account/logout", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = AppFactory.TokenIn(
                        await page.Content.ReadAsStringAsync(), "/change-password")
                }));
            Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
            Assert.Equal("/login", logout.Headers.Location?.OriginalString);
        }
        finally
        {
            SetForced(false);
        }
    }

}
