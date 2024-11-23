﻿using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignUpdateDeliveryPackages;

public class SuggestAssignUpdateDeliveryPackageQuery : IQuery<Result>
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public long[] ShipperIds { get; set; }
}