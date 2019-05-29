﻿using SFA.DAS.EAS.Portal.Client.Types;
using System.Collections.Generic;

namespace SFA.DAS.EAS.Portal.UnitTests.Builders
{
    public class AccountBuilder
    {
        private readonly ICollection<Organisation> _organisations;

        public AccountBuilder()
        {
            _organisations = new List<Organisation>();
        }

        public Account Build()
        {
            return new Account
            {
                Organisations = _organisations
            };
        }

        public AccountBuilder WithOrganisation(Organisation organisation)
        {
            _organisations.Add(organisation);
            return this;
        }

        public static implicit operator Account(AccountBuilder instance)
        {
            return instance.Build();
        }
    }
}