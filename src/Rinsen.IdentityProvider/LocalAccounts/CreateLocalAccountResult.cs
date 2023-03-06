namespace Rinsen.IdentityProvider.LocalAccounts;

public class CreateLocalAccountResult
{

    private CreateLocalAccountResult(LocalAccount? localAccount, bool localAccountAlreadyExist, bool succeeded)
    {
        LocalAccount = localAccount;
        LocalAccountAlreadyExist = localAccountAlreadyExist;
        Succeeded = succeeded;
    }

    public bool LocalAccountAlreadyExist { get;  }
    public bool Succeeded { get; }
    public LocalAccount? LocalAccount { get; }

    public static CreateLocalAccountResult AlreadyExist()
    {
        return new CreateLocalAccountResult(null, localAccountAlreadyExist: true, succeeded: false);
    }

    public static CreateLocalAccountResult Success(LocalAccount localAccount)
    {
        return new CreateLocalAccountResult(localAccount, localAccountAlreadyExist: false, succeeded: true);
    }
}
