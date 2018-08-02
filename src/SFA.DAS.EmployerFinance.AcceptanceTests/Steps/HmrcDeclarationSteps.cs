using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoDi;
using HMRC.ESFA.Levy.Api.Types;
using Moq;
using NServiceBus;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.EAS.TestCommon.TestRepositories;
using SFA.DAS.EmployerFinance.Messages.Commands;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerFinance.AcceptanceTests.Steps
{
    [Binding]
    public class HmrcDeclarationSteps : TechTalk.SpecFlow.Steps
    {
        public const int StepTimeout = 15000;

        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;

        public HmrcDeclarationSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;
        }

        [Given(@"user ([^ ]*) registered as role ([^ ]*) for account ([^ ]*) and added a paye scheme ""(.*)""")]
        public void GivenUserDaveRegisteredAsRoleOwnerForAccountAAndAddedAPayevscheme(string username, string role, string accountName, string payeScheme)
        {
            Given($"user {username} registered");
            And($"user {username} created account {accountName}");
            And($"user {username} has role {role} for account {accountName}");
            And($"user {username} adds paye scheme \"{payeScheme}\" to account {accountName}");
        }

        [Given(@"Hmrc return the following submissions for paye scheme ([^ ]*)")]
        public void GivenIHaveTheFollowingSubmissionsForEmpRef(string empRef, Table table)
        {
            SetPayeSchemeRef(empRef);
            SetupLastEnglishFractionUpdateDate();
            SetupEnglishFractions(empRef, table);
            SetupLevyDeclarations(empRef, table);
        }

        [When(@"we refresh levy data for account ([^ ]*) paye scheme ([^ ]*)")]
        public async Task WhenWeRefreshLevyData(string accountName, string payeScheme)
        {
            var account = _objectContext.Accounts[accountName];

            await _objectContext.FinanceJobsEndPoint.Send(new ImportAccountLevyDeclarationsCommand
            {
                AccountId = account.Id,
                PayeRef = payeScheme
            }).ConfigureAwait(false);

            var cancel = new CancellationTokenSource(StepTimeout);
            var transactionRepository = _objectContainer.Resolve<ITransactionRepository>();

            while (true)
            {
                await Task.Delay(1000, cancel.Token);
                var t = await transactionRepository.GetAccountTransactionSummary(account.Id);
                if (t.Any())
                    break;

                cancel.Token.ThrowIfCancellationRequested();
            }
        }

        [When(@"All the transaction lines in this scenario have had there transaction date updated to their created date")]
        public void WhenScenarioTransactionLinesTransactionDateHaveBeenUpdatedToTheirCreatedDate()
        {
            var transactionRepository = _objectContainer.Resolve<ITestTransactionRepository>();
            transactionRepository.SetTransactionLineDateCreatedToTransactionDate(_objectContext
                .CurrentlyProcessingSubmissionIds.Select(s => s.Key));
        }

        [When(@"All the transaction lines in this scenario have had there transaction date updated to the specified created date")]
        public void WhenAllTheTransactionLinesInThisScenarioHaveHadThereTransactionDateUpdatedToTheSpecifiedCreatedDate()
        {
            var transactionRepository = _objectContainer.Resolve<ITestTransactionRepository>();
            transactionRepository.SetTransactionLineDateCreatedToTransactionDate(_objectContext
                .CurrentlyProcessingSubmissionIds);
        }

        private void SetPayeSchemeRef(string empRef)
        {
            _objectContext.ApprenticeshipLevyApiClient.Setup(x => x.GetEmployerDetails(It.IsAny<string>()))
                .ReturnsAsync(new EmpRefLevyInformation
                {
                    Employer = new Employer()
                    {
                        Name = new Name{EmprefAssociatedName =  $"Name{empRef}"}
                    }
                } );
        }

        private void SetupLastEnglishFractionUpdateDate()
        {
            _objectContext.ApprenticeshipLevyApiClient.Setup(x => x.GetLastEnglishFractionUpdate(It.IsAny<string>()))
                .ReturnsAsync(DateTime.MinValue);
        }

        private void SetupEnglishFractions(string empRef, Table table)
        {
            var fractionCalculations = new List<FractionCalculation>();
            foreach (var tableRow in table.Rows)
            {
                fractionCalculations.Add(new FractionCalculation
                {
                    CalculatedAt = DateTime.Parse(tableRow["SubmissionDate"]),
                    Fractions = new List<Fraction>
                    {
                        new Fraction
                        {
                            Region = "England",
                            Value = tableRow["English_Fraction"]
                        }
                    }
                });
            }

            _objectContext.ApprenticeshipLevyApiClient.Setup(x =>
                    x.GetEmployerFractionCalculations(It.IsAny<string>(), It.Is<string>(s => s.Equals(empRef)), null, null))
                .ReturnsAsync(new EnglishFractionDeclarations
                {
                    Empref = empRef,
                    FractionCalculations = fractionCalculations
                });
        }

        private void SetupLevyDeclarations(string empRef, Table table)
        {
            var levyDeclarations = new LevyDeclarations { EmpRef = empRef, Declarations = new List<Declaration>() };

            var submissionIds = new Dictionary<long, DateTime?>();
            foreach (var tableRow in table.Rows)
            {
                var noPaymentForPeriod = false;
                if (tableRow.ContainsKey("NoPaymentForPeriod"))
                {
                    if (!string.IsNullOrWhiteSpace(tableRow["NoPaymentForPeriod"]))
                        noPaymentForPeriod = Convert.ToBoolean(tableRow["NoPaymentForPeriod"]);
                }

                var submissionId = long.Parse(tableRow["Id"]);

                levyDeclarations.Declarations.Add(new Declaration
                {
                    Id = submissionId.ToString(),
                    SubmissionId = submissionId,
                    NoPaymentForPeriod = noPaymentForPeriod,
                    PayrollPeriod = new PayrollPeriod
                    {
                        Month = Convert.ToInt16(tableRow["Payroll_Month"]),
                        Year = tableRow["Payroll_Year"]
                    },
                    DateCeased = null,
                    InactiveFrom = null,
                    InactiveTo = null,
                    LevyAllowanceForFullYear = 0,
                    LevyDeclarationSubmissionStatus = LevyDeclarationSubmissionStatus.LatestSubmission,
                    LevyDueYearToDate = Convert.ToDecimal(tableRow["LevyDueYtd"]),
                    SubmissionTime = DateTime.Parse(tableRow["SubmissionDate"])
                });

                DateTime? createdDate = null;
                if (tableRow.ContainsKey("CreatedDate") && tableRow["CreatedDate"] != null)
                {
                    createdDate = DateTime.ParseExact(tableRow["CreatedDate"], "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);
                }

                submissionIds.Add(submissionId, createdDate);
            }

            _objectContext.CurrentlyProcessingSubmissionIds = submissionIds;

            _objectContext.ApprenticeshipLevyApiClient.Setup(x => x.GetEmployerLevyDeclarations(It.IsAny<string>(),
                    It.Is<string>(s => s.Equals(empRef)), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(levyDeclarations);
        }
    }
}
