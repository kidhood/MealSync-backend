using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;
using MealSync.Application.UseCases.OptionGroups.Commands.DeleteOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Commands.LinkOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Commands.UnLinkOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroupStatus;
using MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;
using MealSync.Application.UseCases.OptionGroups.Queries.GetListOptionGroupWithFoodLinkStatus;
using MealSync.Application.UseCases.OptionGroups.Queries.GetOptionGroupDetail;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OptionGroupController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateOptionGroup(CreateOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPost(Endpoints.LINK_FOOD_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> LinkFoodOptionGroup([FromBody] LinkOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpGet(Endpoints.GET_ALL_SHOP_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetAllShopOptionGroup([FromQuery] GetAllShopOptionGroupQuery request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.UPDATE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopOptionGroup([FromBody] UpdateOptionGroupCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete(Endpoints.DELETE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> DeleteShopOptionGroup([FromBody] DeleteOptionGroupCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet(Endpoints.GET_DETAIL_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetDetailShopOptionGroup(long id)
    {
        return HandleResult(await Mediator.Send(new GetOptionGroupDetailQuery()
        {
            Id = id
        }));
    }

    [HttpDelete(Endpoints.UNLINK_FOOD_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UnLinkShopOptionGroup([FromBody] UnLinkOptionGroupCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_OPTION_GROUP_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateptionGroupStatus([FromBody] UpdateOptionGroupStatusCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SHOP_OPTION_GROUPS_WITH_LINK_FOOD_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateptionGroupStatus([FromQuery] GetListOptionGroupWithFoodLinkStatusQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }
}