namespace BPMS.Modules.Identity.Services;

public interface IPasswordPolicy
{
    void Validate(string password);
}