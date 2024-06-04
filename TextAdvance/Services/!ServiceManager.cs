using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Services;
public static class ServiceManager
{
    public static IPCProvider IPCProvider { get; private set; }
    public static MoveManager MoveManager { get; private set; }
}
