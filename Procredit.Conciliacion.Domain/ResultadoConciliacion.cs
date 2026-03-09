namespace Procredit.Conciliacion.Domain;

public class ResultadoConciliacion
{
    public bool Conciliado { get; set; }

    public int RegistrosBanRed { get; set; }
    public decimal SumatoriaBanRed { get; set; }

    public int RegistrosProCredit { get; set; }
    public decimal SumatoriaProCredit { get; set; }

    public int DiferenciaRegistros => RegistrosBanRed - RegistrosProCredit;
    public decimal DiferenciaMontos => SumatoriaBanRed - SumatoriaProCredit;

    public List<string> Errores { get; set; } = new();

    public List<string> Advertencias { get; set; } = new();
}
