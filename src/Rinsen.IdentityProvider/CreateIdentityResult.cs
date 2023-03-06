namespace Rinsen.IdentityProvider;

public class CreateIdentityResult
{

    private CreateIdentityResult(Identity? identity, bool succeeded)
    {
        Identity = identity;
        Succeeded = succeeded;
    }

    public Identity? Identity { get; }
    public bool Succeeded { get; }
    public bool IdentityAlreadyExist { get { return !Succeeded; } }

    public static CreateIdentityResult AlreadyExist()
    {
        return new CreateIdentityResult(null, false);
    }

    public static CreateIdentityResult Success(Identity identity)
    {
        return new CreateIdentityResult(identity, true);
    }
}
