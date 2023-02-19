namespace Rinsen.IdentityProvider;

public class IdentityOptions
{
    public IdentityOptions()
    {
        IterationCount = 100000;
        NumberOfBytesInPasswordHash = 128 / 8;
        NumberOfBytesInPasswordSalt = 128 / 8;
        MaxFailedLoginSleepCount = 30;
    }

    public string ConnectionString { get; set; } = string.Empty;
    public int IterationCount { get; set; }
    public int NumberOfBytesInPasswordSalt { get; set; }
    public int NumberOfBytesInPasswordHash { get; set; }
    public int MaxFailedLoginSleepCount { get; set; }

}
