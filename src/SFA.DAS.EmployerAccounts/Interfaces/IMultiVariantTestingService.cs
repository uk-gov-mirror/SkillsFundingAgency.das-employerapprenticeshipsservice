﻿using SFA.DAS.EmployerAccounts.Models.UserView;
using System.Collections.Generic;

namespace SFA.DAS.EmployerAccounts.Interfaces
{
    public interface IMultiVariantTestingService
    {
        MultiVariantViewLookup GetMultiVariantViews();
        string GetRandomViewNameToShow(List<ViewAccess> views);
    }
}
