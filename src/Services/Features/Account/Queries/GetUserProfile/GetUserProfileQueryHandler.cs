using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Account.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler :
        IRequestHandler<GetUserProfileQuery, Result<ProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;
        private readonly IUser _currentUser;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Domain.Resources.Messages> localizer,
            IUser currentUser)
        {
            _userManager = userManager;
            _localizer = localizer;
            _currentUser = currentUser;
        }

        public async Task<Result<ProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return Result<ProfileDto>.NotFound(_localizer["Account_UserNotFound"]);

            var profile = new ProfileDto
            {
                Id = userId,
                Email = user.Email ?? "",
                FullName = user.FullName,
                CPF_CNPJ = user.CPF_CNPJ,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Street = user.Street,
                Number = user.Number,
                Complement = user.Complement,
                Neighborhood = user.Neighborhood,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country
            };

            return Result<ProfileDto>.Success(profile);
        }
    }
}
