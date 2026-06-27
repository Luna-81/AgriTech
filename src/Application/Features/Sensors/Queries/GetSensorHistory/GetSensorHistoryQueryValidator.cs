using FluentValidation;

namespace Application.Features.Sensors.Queries.GetSensorHistory;
public class GetSensorHistoryQueryValidator : AbstractValidator<GetSensorHistoryQuery>
{
    public GetSensorHistoryQueryValidator()
    {
        RuleFor(x => x.SensorId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x=> x)
            .Must(q => q.StartDate == null || q.EndDate == null || q.StartDate <= q.EndDate)
            .WithMessage("The start date cannot be later than the end date.");
    }
}