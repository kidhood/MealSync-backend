using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.WithdrawalRequests.Models;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetAllWithdrawalRequestForMod;

public class GetAllWithdrawalRequestForModHandler : IQueryHandler<GetAllWithdrawalRequestForModQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

    public GetAllWithdrawalRequestForModHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
    }

    public async Task<Result<Result>> Handle(GetAllWithdrawalRequestForModQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var data = await _withdrawalRequestRepository.GetAllWithdrawalRequestForManage(
            dormitoryIds, request.SearchValue, request.DateFrom,
            request.DateTo, request.Status, request.OrderBy,
            request.Direction, request.PageIndex, request.PageSize).ConfigureAwait(false);

        var result = new PaginationResponse<WithdrawalRequestManageDto>(
            data.WithdrawalRequests,
            data.TotalCount,
            request.PageIndex,
            request.PageSize);

        return Result.Success(result);
    }
}