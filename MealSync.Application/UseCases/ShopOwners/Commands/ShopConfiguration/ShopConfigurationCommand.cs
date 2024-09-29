﻿using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.ShopConfiguration;

public class ShopConfigurationCommand : ICommand<Result>
{
    public string Name { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string Description { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int AverageOrderHandleInFrame { get; set; }

    public int AverageOrderHandlerInDay { get; set; }
}