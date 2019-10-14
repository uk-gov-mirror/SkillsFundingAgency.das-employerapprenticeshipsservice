﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EAS.Application.Queries.GetLatestActivities
{
    public class GetLatestActivitiesQuery : IAuthorizationContextModel, IAsyncRequest<GetLatestActivitiesResponse>
    {
        [IgnoreMap]
        [Required]
        public long AccountId { get; set; }
    }
}