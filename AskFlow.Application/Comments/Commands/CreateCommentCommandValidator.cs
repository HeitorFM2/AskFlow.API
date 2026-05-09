using FluentValidation;

namespace AskFlow.Application.Comments.Commands
{
    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("Post inválido.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Conteúdo é obrigatório.")
                .MaximumLength(500).WithMessage("Conteúdo deve ter no máximo 500 caracteres.");
        }
    }
}
