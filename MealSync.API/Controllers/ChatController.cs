﻿using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Foods.Commands.Create;
using MealSync.Application.UseCases.Orders.Queries.GetOrderInforNotification;
using MealSync.Application.UseCases.Orders.Queries.OrderListInforNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ChatController : BaseApiController
{
    [HttpGet(Endpoints.ORDER_INFOR_CHAT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.CustomerClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> GetOrderInforChat(long id)
    {
        return HandleResult(await Mediator.Send(new GetOrderInforChatQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.ORDER_LIST_INFOR_CHAT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.CustomerClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> GetOrderListInforChat([FromQuery] OrderListInforNotificationQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }
}