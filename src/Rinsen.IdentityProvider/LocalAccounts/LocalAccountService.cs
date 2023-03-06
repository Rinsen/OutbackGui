using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class LocalAccountService : ILocalAccountService
{
    private readonly IIdentityAccessor _identityAccessor;
    private readonly ILocalAccountStorage _localAccountStorage;
    private readonly IdentityOptions _options;
    private readonly PasswordHashGenerator _passwordHashGenerator;
    private readonly ILogger<LocalAccountService> _log;

    private static readonly RandomNumberGenerator _cryptoRandom = RandomNumberGenerator.Create();

    public LocalAccountService(IIdentityAccessor identityAccessor,
        ILocalAccountStorage localAccountStorage,
        IdentityOptions options,
        PasswordHashGenerator passwordHashGenerator,
        ILogger<LocalAccountService> log)
    {
        _localAccountStorage = localAccountStorage;
        _identityAccessor = identityAccessor;
        _options = options;
        _passwordHashGenerator = passwordHashGenerator;
        _identityAccessor = identityAccessor;
        _log = log;
    }

    public async Task ChangePasswordAsync(string oldPassword, string newPassword)
    {
        var localAccount = await GetLocalAccountAsync(oldPassword);

        localAccount.PasswordHash = GetPasswordHash(newPassword, localAccount);

        await _localAccountStorage.UpdateAsync(localAccount);
    }

    public async Task<CreateLocalAccountResult> CreateAsync(Guid identityId, string loginId, string password)
    {
        var passwordSalt = new byte[_options.NumberOfBytesInPasswordSalt];
        var totpSecret = new byte[64];
        _cryptoRandom.GetBytes(passwordSalt);
        _cryptoRandom.GetBytes(totpSecret);

        var localAccount = new LocalAccount
        {
            IdentityId = identityId,
            IterationCount = _options.IterationCount,
            PasswordSalt = passwordSalt,
            SharedTotpSecret = totpSecret,
            LoginId = loginId
        };

        localAccount.PasswordHash = GetPasswordHash(password, localAccount);

        try
        {
            await _localAccountStorage.CreateAsync(localAccount);
        }
        catch (IdentityAlreadyExistException e)
        {
            _log.LogError(0, e, "Local account already exist");

            return CreateLocalAccountResult.AlreadyExist();
        }

        return CreateLocalAccountResult.Success(localAccount);
    }

    private byte[] GetPasswordHash(string password, LocalAccount localAccount)
    {
        return _passwordHashGenerator.GetPasswordHash(localAccount.PasswordSalt, password, localAccount.IterationCount, _options.NumberOfBytesInPasswordHash);
    }

    public async Task DeleteLocalAccountAsync(string password)
    {
        var localAccount = await GetLocalAccountAsync(password);

        if (localAccount == null)
        {
            throw new InvalidOperationException("No local account found");
        }

        await _localAccountStorage.DeleteAsync(localAccount);
    }

    private async Task<LocalAccount> GetLocalAccountAsync(string password)
    {
        var localAccount = await _localAccountStorage.GetAsync(_identityAccessor.IdentityId);

        await ValidatePassword(localAccount, password);

        return localAccount;
    }
    
    public async Task<LocalAccount> GetLocalAccountAsync(string loginId, string password)
    {
        var localAccount = await _localAccountStorage.GetAsync(loginId);

        await ValidatePassword(localAccount, password);
        
        if (localAccount.FailedLoginCount > 0)
        {
            await SetFailedLoginCountToZero(localAccount);
        }

        return localAccount;
    }

    private Task SetFailedLoginCountToZero(LocalAccount localAccount)
    {
        localAccount.FailedLoginCount = 0;
        localAccount.Updated = DateTimeOffset.Now;
        return _localAccountStorage.UpdateFailedLoginCountAsync(localAccount);
    }

    public async Task ValidatePasswordAsync(string password)
    {
        var localAccount = await _localAccountStorage.GetAsync(_identityAccessor.IdentityId);
        await ValidatePassword(localAccount, password);
    }

    private async Task ValidatePassword(LocalAccount localAccount, string password)
    {
        if (!localAccount.PasswordHash.SequenceEqual(GetPasswordHash(password, localAccount)))
        {
            await InvalidPassword(localAccount);
        }
    }

    private async Task InvalidPassword(LocalAccount localAccount)
    {
        localAccount.FailedLoginCount++;
        localAccount.Updated = DateTimeOffset.Now;

        _log.LogWarning("Invalid password for local account {0} with iteration count {1}", localAccount.IdentityId, localAccount.IterationCount);

        await _localAccountStorage.UpdateFailedLoginCountAsync(localAccount);

        throw new UnauthorizedAccessException("Invalid password");
    }

    public Task<LocalAccount> GetLocalAccountAsync(Guid identityId)
    {
        return _localAccountStorage.GetAsync(identityId);
    }

    public async Task<string> EnableTotp()
    {
        var localAccount = await _localAccountStorage.GetAsync(_identityAccessor.IdentityId);
        var result = Base32Encoding.ToString(localAccount.SharedTotpSecret);

        localAccount.TwoFactorTotpEnabled = DateTimeOffset.Now;

        await _localAccountStorage.UpdateAsync(localAccount);

        return result;
    }
}
