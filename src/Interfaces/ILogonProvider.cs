using Etrea2.Core;

namespace Etrea2.Interfaces
{
    internal interface ILogonProvider
    {
        void LogonPlayer(ref Descriptor desc);
    }
}
