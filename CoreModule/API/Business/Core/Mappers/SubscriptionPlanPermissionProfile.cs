using AutoMapper;
using Business.Core.DTOs;
using Entities.Core;

namespace Business.Core.Mappers
{
    public class SubscriptionPlanPermissionProfile : Profile
    {
        public SubscriptionPlanPermissionProfile()
        {
            CreateMap<SubscriptionPlanPermission, SubscriptionPlanPermissionDto>();
            CreateMap<CreateSubscriptionPlanPermissionDto, SubscriptionPlanPermission>();
            CreateMap<UpdateSubscriptionPlanPermissionDto, SubscriptionPlanPermission>();
        }
    }
}
