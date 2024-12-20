using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.ShopFood;

public class GetShopFoodHandler : IQueryHandler<GetShopFoodQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopFoodHandler(IFoodRepository foodRepository, IShopRepository shopRepository, IMapper mapper)
    {
        _foodRepository = foodRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopFoodQuery request, CancellationToken cancellationToken)
    {
        var shop = _shopRepository.GetById(request.Id);
        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else if (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { request.Id });
        }
        else
        {
            var data = await _foodRepository.GetShopFood(request.Id, request.SearchValue, request.CategoryId).ConfigureAwait(false);
            var response = data.Select(g => new ShopFoodResponse
            {
                CategoryId = g.CategoryId,
                CategoryName = g.CategoryName,
                Foods = _mapper.Map<List<ShopFoodResponse.FoodResponse>>(g.Foods),
            }).ToList();
            return Result.Success(response);
        }
    }
}