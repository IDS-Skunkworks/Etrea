using Kingdoms_of_Etrea.Core;

namespace Kingdoms_of_Etrea.Interfaces
{
    interface ILogonProvider
    {
        void LogonPlayer(ref Descriptor descriptor);
    }
}
