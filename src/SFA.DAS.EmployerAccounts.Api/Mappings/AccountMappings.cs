namespace SFA.DAS.EmployerAccounts.Api.Mappings
{
    using AutoMapper;

    public class AccountMappings : Profile
    {
        public AccountMappings()
        {
            this.CreateMap<Domain.Models.Account.Account, AccountDetailViewModel>()
                .ForMember(target => target.HashedAccountId, opt => opt.MapFrom(src => src.HashedId))
                .ForMember(target => target.DasAccountName, opt => opt.MapFrom(src => src.Name));
            this.CreateMap<LevyDeclarationView, LevyDeclarationViewModel>()
                .ForMember(target => target.PayeSchemeReference, opt => opt.MapFrom(src => src.EmpRef));
            this.CreateMap<Domain.Models.EmployerAgreement.EmployerAgreementView, EmployerAgreementView>();
        }
    }
}