using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.Outback.Entities;

public interface ISoftDelete
{
    public DateTimeOffset? Deleted { get; set; }
}
