using System.Web.Mvc;
using SFA.DAS.EmployerFinance.Web.Binders;

namespace SFA.DAS.EmployerFinance.Web
{
    public class BinderConfig
    {
        public static void RegisterBinders(ModelBinderDictionary binders)
        {
            binders.Add(typeof(string), new TrimStringModelBinder());
        }
    }
}
