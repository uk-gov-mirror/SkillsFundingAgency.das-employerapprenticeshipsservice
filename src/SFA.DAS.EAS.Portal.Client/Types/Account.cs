using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EAS.Portal.Client.Types
{
    public class Account
    {
        public Account()
        {
            Organisations = new List<Organisation>();
            PayeSchemes = new List<PayeScheme>();
            Providers = new List<Provider>();
        }

        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("organisations")]
        public ICollection<Organisation> Organisations { get; set; }
        [JsonProperty("providers")]
        public ICollection<Provider> Providers { get; set; }
        [JsonProperty("deleted")]
        public DateTime? Deleted { get; set; }
        [JsonProperty("payeSchemes")]
        public ICollection<PayeScheme> PayeSchemes { get; set; }
    }
}
