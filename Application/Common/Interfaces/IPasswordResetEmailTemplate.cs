namespace Application.Common.Interfaces
{
    public interface IPasswordResetEmailTemplate
    {
        string Build(string fullName, string temporaryPassword);
    }
}
