namespace SFA.DAS.EmployerAccounts.Models.Account
{
    public enum AccountLevel
    {
        BasicAccount = 1,
        AccountHasNoPAYE = 2,
        AccountHasPAYEAndOrganisationsButNoSignedAgreements = 3,
        AccountHasPAYEAndSomeOrganisationsHaveSignedAgreements = 4,
        AccountHasPAYEAndAllOrganisationsHaveSignedAgreements = 5
    }
}