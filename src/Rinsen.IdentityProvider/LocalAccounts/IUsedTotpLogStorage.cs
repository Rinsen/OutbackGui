using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts;

public interface IUsedTotpLogStorage
{
    Task CreateLog(UsedTotpLog usedTotp);

}
