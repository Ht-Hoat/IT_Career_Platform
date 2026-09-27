using System.Globalization;
using Microsoft.AspNetCore.DataProtection;

namespace ITCareerPlatform.Services;

/// <summary>Mật khẩu tạm đã đi tới Admin bằng đường nào.</summary>
public enum ResetDelivery
{
    /// <summary>Đã gửi qua email — Admin không cần nhìn thấy mật khẩu.</summary>
    Emailed = 0,
    /// <summary>Chưa cấu hình SMTP: hiện trên màn hình đúng một lần.</summary>
    Shown = 1,
    /// <summary>Đã thử gửi email nhưng hỏng — vẫn phải hiện, kèm lời cảnh báo.</summary>
    ShownAfterMailFailure = 2
}

/// <summary>
/// Kết cục của MỘT lần đặt lại mật khẩu, phát biểu đúng một lần.
/// <para><see cref="Password"/> chỉ có giá trị khi Admin phải tự đọc nó cho người dùng.</para>
/// </summary>
public sealed record ResetHandoff(ResetDelivery Delivery, bool Manual, string TargetEmail, string? Password);

/// <summary>
/// Chuyển KẾT CỤC của lần đặt lại mật khẩu từ endpoint reset-password sang trang /users để
/// hiện ĐÚNG MỘT LẦN cho Admin.
///
/// Toàn bộ mô tả kết cục nằm ở đây, không rải thành cờ trên query string. Bốn cờ boolean
/// (sent/manual/tempPw/mailfailed) cho 16 tổ hợp danh nghĩa trên 5 kết cục có thật, và trang
/// phải tự phòng thủ trước những tổ hợp không thể xảy ra — đó là cách hộp thoại từng hiện
/// một ô mật khẩu RỖNG. Ở đây chỉ có một giá trị, hoặc có hoặc không.
///
/// Không đi qua query string: thanh địa chỉ nằm lại trong lịch sử trình duyệt của Admin và
/// trong access log của mọi proxy đứng giữa, và ở đó nó là một mật khẩu dùng được.
/// Thay vào đó là một cookie:
///   - mã hóa + ký bằng Data Protection, hết hạn sau <see cref="Lifetime"/>;
///   - HttpOnly, SameSite=Strict, chỉ gửi kèm đường dẫn /users;
///   - gắn với Id của Admin đã bấm reset — Admin khác dùng chung máy không đọc được;
///   - bị xóa ngay lần đọc đầu tiên, nên tải lại trang là mật khẩu biến mất.
/// </summary>
public static class TempPasswordHandoff
{
    public const string CookieName = "ITCP.TempPw";
    private const string Purpose = "ITCareerPlatform.TempPasswordHandoff.v1";
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    public static void Store(HttpContext ctx, IDataProtectionProvider dp, int adminUserId, ResetHandoff result)
    {
        // Mật khẩu để CUỐI: chính Admin gõ nó, nên nó có thể chứa dấu '|'. Bốn trường đầu
        // thì không (số, số, số, email), nên tách tối đa 5 phần là đọc lại đúng nguyên văn.
        var plain = string.Join('|',
            adminUserId.ToString(CultureInfo.InvariantCulture),
            ((int)result.Delivery).ToString(CultureInfo.InvariantCulture),
            result.Manual ? "1" : "0",
            result.TargetEmail,
            result.Password ?? "");

        var payload = dp.CreateProtector(Purpose).ToTimeLimitedDataProtector().Protect(plain, Lifetime);
        ctx.Response.Cookies.Append(CookieName, payload, CookieOptions(ctx));
    }

    /// <summary>Đọc rồi xóa. Trả null khi không có, đã hết hạn, bị sửa, hoặc thuộc Admin khác.</summary>
    public static ResetHandoff? TakeOnce(HttpContext ctx, IDataProtectionProvider dp, int adminUserId)
    {
        if (!ctx.Request.Cookies.TryGetValue(CookieName, out var payload) || string.IsNullOrEmpty(payload))
            return null;

        // Xóa trước khi giải mã: dù giá trị hỏng hay hợp lệ thì cũng không được đọc lần hai.
        if (!ctx.Response.HasStarted)
            ctx.Response.Cookies.Delete(CookieName, CookieOptions(ctx));

        try
        {
            var parts = dp.CreateProtector(Purpose).ToTimeLimitedDataProtector().Unprotect(payload).Split('|', 5);
            if (parts.Length != 5) return null;
            if (parts[0] != adminUserId.ToString(CultureInfo.InvariantCulture)) return null;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var delivery)
                || !Enum.IsDefined(typeof(ResetDelivery), delivery)) return null;

            return new ResetHandoff((ResetDelivery)delivery, parts[2] == "1", parts[3],
                                    parts[4].Length == 0 ? null : parts[4]);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;   // hết hạn hoặc bị sửa
        }
    }

    private static CookieOptions CookieOptions(HttpContext ctx) => new()
    {
        HttpOnly = true,
        Secure = ctx.Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = "/users",
        MaxAge = Lifetime
    };
}
