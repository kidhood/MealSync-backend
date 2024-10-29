﻿using System.Net;
using System.Runtime.Intrinsics.Arm;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;

public class ShopCreateDeliveryPackageHandler : ICommandHandler<ShopCreateDeliveryPackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ILogger<ShopCreateDeliveryPackageHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IDapperService _dapperService;
    private readonly IMapper _mapper;

    public ShopCreateDeliveryPackageHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IDeliveryPackageRepository deliveryPackageRepository, ILogger<ShopCreateDeliveryPackageHandler> logger,
        INotificationFactory notificationFactory, INotifierService notifierService, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IShopRepository shopRepository,
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository, IDapperService dapperService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _dapperService = dapperService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopCreateDeliveryPackageCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        var order = _orderRepository.GetById(request.DeliveryPackages.First().OrderIds.First());
        if (!request.IsConfirm.Value)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var intendedReceiveDateTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS);
            if (now < endTime)
            {
                var diffDate = endTime.AddHours(OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS) - now;
                return Result.Success(new
                {
                    Code = MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                        new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), $"{diffDate.Hours}:{diffDate.Minutes}" }),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var deliveryPackages = new List<DeliveryPackage>();
            foreach (var deliveryPackageRequest in request.DeliveryPackages)
            {
                var deliveryPackage = await CreateOrderSaveDeliveryPackageAsync(deliveryPackageRequest).ConfigureAwait(false);
                deliveryPackages.Add(deliveryPackage);
            }

            await _deliveryPackageRepository.AddRangeAsync(deliveryPackages).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var listNoti = new List<Notification>();
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);

            // Noti to shop staff about delivery package
            foreach (var dp in deliveryPackages)
            {
                if (dp.ShopDeliveryStaffId.HasValue)
                {
                    var accShip = _accountRepository.GetById(dp.ShopDeliveryStaffId.Value);
                    var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                    listNoti.Add(notiShopStaff);
                }
            }

            _notifierService.NotifyRangeAsync(listNoti);
            var response = _mapper.Map<List<DeliveryPackageResponse>>(deliveryPackages);
            foreach (var deliveryPackageResponse in response)
            {
                deliveryPackageResponse.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackageResponse.Id).ConfigureAwait(false);
            }

            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<DeliveryPackage> CreateOrderSaveDeliveryPackageAsync(DeliveryPackageRequest request)
    {
        var orders = _orderRepository.Get(o => request.OrderIds.Contains(o.Id)).ToList();

        // Need create new delivery package
        var dp = new DeliveryPackage()
        {
            ShopDeliveryStaffId = request.ShopDeliveryStaffId,
            ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
            DeliveryDate = TimeFrameUtils.GetCurrentDateInUTC7().Date,
            StartTime = orders.First().StartTime,
            EndTime = orders.First().EndTime,
            Status = DeliveryPackageStatus.Created,

        };
        dp.Orders = orders;

        if (request.ShopDeliveryStaffId != null)
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
            shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
            _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
        }

        return dp;
    }

    private void Validate(ShopCreateDeliveryPackageCommand request)
    {
        var listOrder = new List<Order>();
        foreach (var deliveryPackage in request.DeliveryPackages)
        {
            foreach (var orderId in deliveryPackage.OrderIds)
            {
                var order = _orderRepository
                    .Get(o => o.Id == orderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                    .Include(o => o.DeliveryPackage).SingleOrDefault();

                if (order == default)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { orderId }, HttpStatusCode.NotFound);

                if (order.Status != OrderStatus.Preparing)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { orderId });

                if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });

                if (order.DeliveryPackageId != null)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_IN_OTHER_PACKAGE.GetDescription(), new object[] { orderId });

                listOrder.Add(order);
            }

            var firstOrderInPackage = listOrder.FirstOrDefault() ?? new Order();
            var shipperId = deliveryPackage.ShopDeliveryStaffId.HasValue ? deliveryPackage.ShopDeliveryStaffId.Value : _currentPrincipalService.CurrentPrincipalId.Value;
            var deliveryPackageCheck = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(!deliveryPackage.ShopDeliveryStaffId.HasValue,
                shipperId, firstOrderInPackage.StartTime, firstOrderInPackage.EndTime);
            if (deliveryPackageCheck != null)
            {
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_ALREADY_HAVE_OTHER_PACKAGE.GetDescription(), new object[] { shipperId });
            }
        }

        var differentOrders = GetDifferentTimeOrders(listOrder);
        if (differentOrders != null)
        {
            var listOrderIds = differentOrders.Select(x => string.Concat(IdPatternConstant.PREFIX_ID, x.Id)).ToList();
            var joinListOrderIds = string.Join(',', listOrderIds);
            var firstOrderCompare = listOrder.FirstOrDefault() ?? new Order();
            throw new InvalidBusinessException(MessageCode.E_ORDER_IN_DIFFERENT_FRAME.GetDescription(), new object[]
            {
                joinListOrderIds, TimeFrameUtils.GetTimeFrameString(firstOrderCompare.StartTime, firstOrderCompare.EndTime),
            });
        }
    }

    public List<Order> GetDifferentTimeOrders(List<Order> orders)
    {
        // Check if the list is empty or only contains one order (no comparison needed)
        if (orders == null || orders.Count <= 1)
            return null;

        // Get the reference times from the first order
        var firstOrder = orders.First();
        var startTime = firstOrder.StartTime;
        var endTime = firstOrder.EndTime;

        // Find orders with different StartTime or EndTime
        var differentOrders = orders
            .Where(o => o.StartTime != startTime || o.EndTime != endTime)
            .ToList();

        // Return the list of different orders, or null if there are none
        return differentOrders.Any() ? differentOrders : null;
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderByDeliveryPackageIdAsync(long packageId)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse> map = (parent, child1, child2) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.Customer = child1;
                parent.Foods.Add(child2);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child2);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        await _dapperService.SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse>(
            QueryName.GetListOrderByPackageId,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                DeliveryPackageId = packageId,
            },
            "CustomerSection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }
}