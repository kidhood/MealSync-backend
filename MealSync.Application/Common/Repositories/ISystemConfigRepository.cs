﻿using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface ISystemConfigRepository : IBaseRepository<SystemConfig>
{
    SystemConfig GetSystemConfig();
}