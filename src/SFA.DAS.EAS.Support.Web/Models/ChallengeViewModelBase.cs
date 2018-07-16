using System;
using System.Collections.Generic;
using SFA.DAS.Support.Shared.Navigation;

namespace SFA.DAS.EAS.Support.Web.Models
{
    /// <summary>
    /// Models the data for a challenge flow, menu persistence, and return to and continue on success 
    /// </summary>
    public abstract class ChallengeViewModelBase
    {
        /// <summary>
        /// Unique Identifier for Challenge lifetime management
        /// </summary>
        public Guid ChallengeId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// The menu to display during the challenge
        /// </summary>
        public SupportMenuPerspectives MenuType { get; set; }
        /// <summary>
        /// The menu identifiers to display during the challenge
        /// </summary>
        public Dictionary<string, string> Identifiers { get; set; }

        /// <summary>
        /// The entity Key value
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The On success return uri
        /// </summary>
        public string ReturnTo { get; set; }
        /// <summary>
        /// The Challenge header
        /// </summary>
        public string Challenge { get; set; }
        /// <summary>
        /// The  Challenge postback Uri
        /// </summary>
        public string ResponseUrl { get; set; }
        /// <summary>
        /// The type of Entity being accessed (Account|WHATEVER)
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// The callers Identity (Email)
        /// </summary>
        public string Identity { get; set; }
        /// <summary>
        /// The current tries attempted
        /// </summary>
        public int Tries { get; set; } = 1;

        /// <summary>
        /// The maximum number for tries before denial of access
        /// </summary>
        public int MaxTries { get; set; } = 3;
        /// <summary>
        /// The challenge failure message
        /// </summary>
        public string Message { get; set; }
       
       
    }
}