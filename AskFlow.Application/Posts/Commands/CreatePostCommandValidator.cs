using FluentValidation;

namespace AskFlow.Application.Posts.Command
{
    public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        public CreatePostCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Conteúdo é obrigatório.")
                .MaximumLength(280).WithMessage("Conteúdo deve ter no máximo 280 caracteres.");
        }
    }
}
