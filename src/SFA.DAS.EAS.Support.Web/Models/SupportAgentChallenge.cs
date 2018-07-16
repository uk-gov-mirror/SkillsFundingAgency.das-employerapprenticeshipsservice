using System;

namespace SFA.DAS.EAS.Support.Web.Models
{
    public class SupportAgentChallenge
    {
        public Guid Id { get;set; }
        public string Identity { get; set; }
        public string EntityType { get; set; }
        public string EntityKey { get; set; }
        public DateTimeOffset Expires { get; set; } 
        
    }
}