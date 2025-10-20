using FluentValidation;

namespace BuildingBlocks.DTOs.Validators;

public class SolicitudProcesaPedidoDtoValidator : AbstractValidator<SolicitudProcesaPedidoDto>
{
    public SolicitudProcesaPedidoDtoValidator()
    {
        RuleFor(x => x.IdCliente).GreaterThan(0);
        RuleFor(x => x.MontoPago).GreaterThan(0);
        RuleFor(x => x.FormaPago).InclusiveBetween(1, 3);
    }
}
