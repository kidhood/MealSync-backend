using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Shops.Commands.ModeratorManage.UpdateShopStatus;

public class UpdateShopStatusHandler : ICommandHandler<UpdateShopStatusCommand, Result>
{

    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateShopStatusHandler> _logger;

    public UpdateShopStatusHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IShopRepository shopRepository, IUnitOfWork unitOfWork, ISystemResourceRepository systemResourceRepository,
        ILogger<UpdateShopStatusHandler> logger, IEmailService emailService)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _shopRepository = shopRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<Result<Result>> Handle(UpdateShopStatusCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();
        var isSendMailApprove = false;

        var shop = await _shopRepository.GetShopManage(request.ShopId, dormitoryIds).ConfigureAwait(false);
        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            if ((shop.Status == ShopStatus.UnApprove && request.Status == ShopStatus.InActive)
                || ((shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned) && request.Status == ShopStatus.InActive)
                || (shop.Status != ShopStatus.Banned && request.Status == ShopStatus.Banned))
            {
                if (shop.Status == ShopStatus.UnApprove && request.Status == ShopStatus.InActive)
                {
                    isSendMailApprove = true;
                }
                shop.Status = request.Status;
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _shopRepository.Update(shop);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                if (request.Status == ShopStatus.Banned)
                {
                    _emailService.SendEmailToAnnounceAccountGotBanned(shop.Account.Email, shop.Account.FullName);
                }
                else if (isSendMailApprove)
                {
                    _emailService.SendApproveShop(shop.Account.Email, shop.Account.FullName, shop.Name);
                }

                return Result.Success(new
                {
                    Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
    }
}