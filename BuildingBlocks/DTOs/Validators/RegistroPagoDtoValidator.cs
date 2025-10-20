using FluentValidation;

namespace BuildingBlocks.DTOs.Validators;

public class RegistroPagoDtoValidator : AbstractValidator<RegistroPagoDto>
{
    public RegistroPagoDtoValidator()
    {
        RuleFor(x => x.IdCliente).GreaterThan(0);
        RuleFor(x => x.IdPedido).GreaterThan(0);
        RuleFor(x => x.MontoPago).GreaterThan(0);
        RuleFor(x => x.FormaPago).InclusiveBetween(1, 3);
    }
}
