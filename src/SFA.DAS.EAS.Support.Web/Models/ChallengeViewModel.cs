using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EAS.Support.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class PayeSchemeChallengeViewModel : ChallengeViewModelBase
    {
        
        public List<int> Characters { get; set; }

        public string Balance { get; set; }

        public bool HasError { get; set; }
        public string Challenge1 { get; set; }
        public string Challenge2 { get; set; }
        public int FirstCharacterPosition { get; set; }
        public int SecondCharacterPosition { get; set; }
    }
}