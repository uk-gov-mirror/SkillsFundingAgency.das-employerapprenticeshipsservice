using SFA.DAS.EmployerFinance.Models.UserView;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface IMultiVariantTestingService
    {
        MultiVariantViewLookup GetMultiVariantViews();
        string GetRandomViewNameToShow(List<ViewAccess> views);
    }
}
