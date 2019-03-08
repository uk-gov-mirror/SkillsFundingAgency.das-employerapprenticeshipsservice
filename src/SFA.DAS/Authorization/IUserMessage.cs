using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IUserMessage
    {
        Guid? UserRef { get; set; }
    }
}