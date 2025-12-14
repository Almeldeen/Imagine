using System;

namespace Infrastructure.Services
{
    internal static class EmailTemplates
    {
        public static string BuildPasswordResetHtml(string fullName, string temporaryPassword)
        {
            var safeName = string.IsNullOrWhiteSpace(fullName) ? "" : fullName.Trim();

            return $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1' />
</head>
<body style='margin:0;padding:0;background:#f5f7fb;font-family:Arial,Helvetica,sans-serif;'>
  <div style='max-width:640px;margin:0 auto;padding:24px;'>
    <div style='background:#0b1220;border-radius:14px;padding:22px 22px 18px;color:#ffffff;'>
      <div style='font-size:18px;font-weight:700;letter-spacing:0.3px;'>Imagine</div>
      <div style='opacity:0.85;margin-top:6px;'>Your password has been reset</div>
    </div>

    <div style='background:#ffffff;border-radius:14px;margin-top:14px;padding:22px;border:1px solid #e8eef7;'>
      <p style='margin:0 0 12px 0;color:#111827;font-size:14px;'>Hi {System.Net.WebUtility.HtmlEncode(safeName)},</p>
      <p style='margin:0 0 14px 0;color:#111827;font-size:14px;'>An administrator has reset your password. Use the temporary password below to log in:</p>

      <div style='background:#f3f4f6;border:1px dashed #cbd5e1;border-radius:10px;padding:14px;margin:14px 0;'>
        <div style='font-size:12px;color:#64748b;margin-bottom:6px;'>Temporary password</div>
        <div style='font-family:Consolas,Monaco,monospace;font-size:18px;color:#0f172a;font-weight:700;letter-spacing:0.4px;'>
          {System.Net.WebUtility.HtmlEncode(temporaryPassword)}
        </div>
      </div>

      <p style='margin:0 0 14px 0;color:#111827;font-size:14px;'><strong>Important:</strong> Please change your password immediately after logging in.</p>

      <p style='margin:0;color:#64748b;font-size:13px;'>If you did not request this change, please contact support.</p>
    </div>

    <div style='text-align:center;color:#94a3b8;font-size:12px;margin-top:14px;'>
      <div>Need help? Contact us at <a href='mailto:support@imagine.com' style='color:#64748b;'>support@imagine.com</a></div>
      <div style='margin-top:6px;'>© {DateTime.UtcNow.Year} Imagine</div>
    </div>
  </div>
</body>
</html>";
        }
    }
}
