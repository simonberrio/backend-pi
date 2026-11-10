namespace Repositories.Models
{
    // Estados de una transacción en Wompi
    public enum PaymentStatusEnums
    {
        Pending,
        Approved,
        Declined,
        Voided,
        Error
    }
}
