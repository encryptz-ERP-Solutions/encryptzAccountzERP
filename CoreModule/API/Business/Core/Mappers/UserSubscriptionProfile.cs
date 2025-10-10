using AutoMapper;
using Business.Core.DTOs;
using Entities.Core;

namespace Business.Core.Mappers
{
    public class UserSubscriptionProfile : Profile
    {
        public UserSubscriptionProfile()
        {
            CreateMap<UserSubscription, UserSubscriptionDto>();
            CreateMap<CreateUserSubscriptionDto, UserSubscription>();
            CreateMap<UpdateUserSubscriptionDto, UserSubscription>();
        }
    }
}
