using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class PasswordResetEmailTemplate : IPasswordResetEmailTemplate
    {
        public string Build(string fullName, string temporaryPassword)
        {
            return EmailTemplates.BuildPasswordResetHtml(fullName, temporaryPassword);
        }
    }
}
