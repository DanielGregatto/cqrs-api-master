using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;

namespace Services.Features.Account.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<ProfileDto>>
    {
    }
}
