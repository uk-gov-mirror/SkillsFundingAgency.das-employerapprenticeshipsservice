using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface ICallerContextProvider
    {
        ICallerContext GetCallerContext();
    }
}