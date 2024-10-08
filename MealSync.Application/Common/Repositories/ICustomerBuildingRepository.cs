﻿using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface ICustomerBuildingRepository : IBaseRepository<CustomerBuilding>
{
    CustomerBuilding? GetDefaultByCustomerId(long id);

    CustomerBuilding? GetByBuildingIdAndCustomerId(long buildingId, long customerId);

    CustomerBuilding GetByBuildingIdAndCustomerIdIncludeBuilding(long buildingId, long customerId);
}