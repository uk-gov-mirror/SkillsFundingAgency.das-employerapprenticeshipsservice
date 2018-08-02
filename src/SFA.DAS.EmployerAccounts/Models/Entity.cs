﻿using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Models
{
    public abstract class Entity
    {
        protected void Publish<T>(Action<T> action) where T : Event, new()
        {
            UnitOfWorkContext.AddEvent(action);
        }
    }
}