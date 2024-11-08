﻿using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Options.Commands.CreateNewOption;

public class CreateNewOptionCommand : ICommand<Result>
{
    public long OptionGroupId { get; set; }

    public bool IsDefault { get; set; }

    public string Title { get; set; } = null!;

    public bool IsCalculatePrice { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }
}