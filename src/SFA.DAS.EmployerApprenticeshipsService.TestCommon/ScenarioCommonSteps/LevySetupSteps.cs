using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoDi;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.Levy;
using SFA.DAS.EAS.Domain.Models.Payments;
using SFA.DAS.EAS.TestCommon.Extensions;
using SFA.DAS.EAS.TestCommon.Steps;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.ScenarioCommonSteps
{
    [Binding]
    public class LevySetupSteps
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;

        private readonly IDasLevyRepository _dasLevyRepository;

        public LevySetupSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;

            var container = objectContainer.Resolve<IContainer>();

            _dasLevyRepository = container.GetInstance<IDasLevyRepository>();
        }

        [Given(@"a transfer allowance of (.*) is set for Paye Scheme ""(.*)"" for account (.*)")]
        public async Task GivenATransferAllowanceOfIsSetForPayeSchemeForAccountA(decimal transferAllowance, string payeScheme, string accountName)
        {
            var account = _objectContext.Accounts[accountName];

            // Need to add transaction lines for LAST year
            var date = DateTime.UtcNow.AddYears(-1);

            await EnsurePeriodEndExists(date);

            await _dasLevyRepository.CreateEmployerDeclarations(new List<DasDeclaration>
            {
                new DasDeclaration
                {
                    Id = date.Ticks.ToString(),
                    SubmissionId = date.Ticks,
                    SubmissionDate = date,
                    PayrollYear = date.Year.ToString(),
                    PayrollMonth = (short)date.Month,
                    LevyDueYtd = transferAllowance
                }
            }, payeScheme, account.Id);

            await _dasLevyRepository.ProcessDeclarations(account.Id, payeScheme);

            _objectContext.Accounts[accountName] = account;
        }

        /// <summary>
        /// We need to add a period end if it is missing
        /// </summary>
        private async Task EnsurePeriodEndExists(DateTime date)
        {
            var periodEnds = await _dasLevyRepository.GetAllPeriodEnds();
            if (!periodEnds.Any(pe => pe.CalendarPeriodMonth == date.Month && pe.CalendarPeriodYear == date.Year))
            {
                await _dasLevyRepository.CreateNewPeriodEnd(new PeriodEnd
                {
                    PeriodEndId = date.ToPeriodEndId(),
                    CalendarPeriodMonth = date.Month,
                    CalendarPeriodYear = date.Year,
                    CompletionDateTime = date,
                    PaymentsForPeriod = "url"
                });
            }
        }
    }
}
