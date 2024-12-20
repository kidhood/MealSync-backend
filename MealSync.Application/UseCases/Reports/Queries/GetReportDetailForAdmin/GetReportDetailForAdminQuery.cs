using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForAdmin;

public class GetReportDetailForAdminQuery : IQuery<Result>
{
    public long ReportId { get; set; }
}