using FluentValidation;
using HeartCheck.DTOs.Measurements;
using MongoDB.Bson;

namespace HeartCheck.Validators
{
    public class CreateMeasurementRequestValidator : AbstractValidator<CreateMeasurementRequest>
    {
        private static readonly string[] ValidContexts = { "rest", "active", "sleep" };

        public CreateMeasurementRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("DeviceId is required")
                .Must(BeValidObjectId).WithMessage("DeviceId must be a valid ObjectId");

            RuleFor(x => x.Bpm)
                .InclusiveBetween(30, 250)
                .WithMessage("Bpm must be between 30 and 250");

            RuleFor(x => x.Quality)
                .NotEmpty().WithMessage("Quality is required")
                .MaximumLength(20).WithMessage("Quality must not exceed 20 characters");

            RuleFor(x => x.Context)
                .NotEmpty().WithMessage("Context is required")
                .Must(BeValidContext).WithMessage("Context must be one of: rest, active, sleep");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");

            RuleForEach(x => x.Symptoms)
                .NotEmpty().WithMessage("Symptoms must not contain empty values")
                .MaximumLength(100).WithMessage("Each symptom must not exceed 100 characters");
        }

        private static bool BeValidObjectId(string value)
        {
            return ObjectId.TryParse(value, out _);
        }

        private static bool BeValidContext(string context)
        {
            return ValidContexts.Contains(context.ToLowerInvariant());
        }
    }
}
