﻿using System;
using Newtonsoft.Json;

namespace SFA.DAS.EmployerAccounts.Models
{
    public class Job : Entity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public DateTime Completed { get; private set; }

        public Job(int id, string name, DateTime completed)
        {
            Id = id;
            Name = name;
            Completed = completed;
        }

        [JsonConstructor]
        private Job()
        {
        }
    }
}