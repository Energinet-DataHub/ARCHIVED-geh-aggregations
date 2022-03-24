using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace DAL
{
    public interface IMeteringPointRepository
    {
        Task<List<MeteringPoint>> GetByIdAndDateAsync(string id, DateTime effectiveDate);

        Task AddOrUpdateMeteringPoints(List<MeteringPoint> meteringPoints);
    }
}
