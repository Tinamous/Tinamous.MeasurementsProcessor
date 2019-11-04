using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.DAL.Interfaces
{
    public interface IMeasurementUserPropertiesRepository
    {
        Task CreateTableAsync();
        Task<List<User>> LoadAsync(Guid accountId);
        Task<User> LoadAsync(Guid accountId, Guid userId);
        Task SaveAsync(User user);
        Task DeleteAsync(User userProperties);
    }
}