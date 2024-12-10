﻿using System.Text.Json.Serialization;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

public class DeliveryPackageForAssignUpdateResponse
{
    public long DeliveryPackageId { get; set; }

    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    [JsonIgnore]
    public int StartTime { get; set; }

    [JsonIgnore]
    public int EndTime { get; set; }

    // [JsonIgnore]
    public double CurrentDistance { get; set; }

    // [JsonIgnore]
    public double CurrentTaskLoad {
        get
        {
            return DeliveryPackageWeight;
        }
    }

    public double DeliveryPackageWeight
    {
        get
        {
            return Orders.Sum(o => o.TotalWeight);
        }
    }

    // [JsonIgnore]
    public int TotalMinutesHandleDelivery
    {
        get
        {
            return TotalMinutesToWaitCustomer
                   + (int)(Math.Ceiling((double)Total / 5) * DevidedOrderConstant.MinutesAddWhenOrderMoreThanFive)
                   + TotalMinutestToMove;
        }
    }

    // [JsonIgnore]
    public int TotalMinutestToMove { get; set; }

    // [JsonIgnore]
    public int TotalMinutesToWaitCustomer { get; set; }

    public int SuggestStartTimeDelivery
    {
        get
        {
            var endTime = TimeFrameUtils.GetStartTimeToDateTime(DateTime.Now, EndTime);

            endTime = endTime.AddMinutes(-TotalMinutesHandleDelivery);
            return int.Parse(endTime.ToString("HHmm"));
        }
    }

    public DeliveryPackageForAssignResponse.ShopStaffInforResponse ShopDeliveryStaff { get; set; }

    public List<DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff> Dormitories { get; set; } = new();

    public List<OrderDetailForShopResponse> Orders { get; set; } = new();
}