using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Services.Extensions
{
    public static class UserExtension
    {
        public static UserSummaryDto ToUserSummaryDto(this User user)
        {
            return new UserSummaryDto
            {
                AccountId = user.AccountId,
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.DisplayName,
                FullUserName = user.FullUserName,
            };
        }
    }
}