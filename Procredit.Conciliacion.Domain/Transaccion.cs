namespace Procredit.Conciliacion.Domain;

public class Transaccion
{
    public string CuentaOrigen { get; set; } = string.Empty;
    public string CuentaDestino { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string NumIdentificacion { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public string FechaRegistro { get; set; } = string.Empty;

    public static readonly string[] ColumnasEsperadas =
    {
        "cuenta origen",
        "cuenta destino",
        "monto",
        "motivo",
        "num identificacion benef",
        "nombre benef",
        "fecha registro"
    };

    public bool IdentificacionValida =>
        NumIdentificacion.Length == 10 || NumIdentificacion.Length == 13;

    public bool MontoValido => Monto > 0;
}
