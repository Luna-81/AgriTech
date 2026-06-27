using FluentValidation;

namespace Application.Features.Farms.Commands.CreateFarm;

public class CreateFarmCommandValidator : AbstractValidator<CreateFarmCommand>
{
    public CreateFarmCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("农场名称不能为空。")
            .MaximumLength(100).WithMessage("农场名称不能超过100个字符。");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("纬度必须在 -90 到 90 度之间。");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("经度必须在 -180 到 180 度之间。");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("农场容量必须大于0。")
            .LessThanOrEqualTo(1000).WithMessage("农场容量不能超过1000。");
    }
}