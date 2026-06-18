using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.User;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record UploadUserPhotoCommand(Guid UserId, byte[] Photo, string ContentType) : IRequest<UserResponse?>;

public class UploadUserPhotoCommandHandler(IUserRepository repository)
    : IRequestHandler<UploadUserPhotoCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(UploadUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return null;

        user.SetPhoto(request.Photo, request.ContentType);
        await repository.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
}

public class UploadUserPhotoCommandValidator : AbstractValidator<UploadUserPhotoCommand>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public UploadUserPhotoCommandValidator()
    {
        RuleFor(x => x.Photo).NotEmpty().WithMessage("Photo is required.");
        RuleFor(x => x.Photo).Must(p => p.Length <= 5 * 1024 * 1024)
            .WithMessage("Photo must not exceed 5 MB.");
        RuleFor(x => x.ContentType).Must(t => AllowedTypes.Contains(t))
            .WithMessage("Only JPEG, PNG, WebP and GIF images are allowed.");
    }
}
