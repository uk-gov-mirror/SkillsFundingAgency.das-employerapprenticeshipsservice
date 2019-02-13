namespace SFA.DAS.EmployerAccounts.Api.Mappings
{
    using AutoMapper;

    public class UserMappings : Profile
    {
        public UserMappings()
        {
            this.CreateMap<TeamMember, TeamMemberViewModel>();
        }
    }
}