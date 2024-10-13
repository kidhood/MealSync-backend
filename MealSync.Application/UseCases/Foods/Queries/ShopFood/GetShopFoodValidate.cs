using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.ShopFood;

public class GetShopFoodValidate : AbstractValidator<GetShopFoodQuery>
{
    public GetShopFoodValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}