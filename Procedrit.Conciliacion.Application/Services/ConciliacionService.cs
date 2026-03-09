using Procredit.Conciliacion.Domain;
using Serilog;

namespace Procedrit.Conciliacion.Application.Services;

public class ConciliacionService
{
    public ResultadoConciliacion Conciliar(
        List<Transaccion> banred,
        List<Transaccion> procredit)
    {
        Log.Information("Iniciando proceso de conciliación...");

        var resultado = new ResultadoConciliacion
        {
            RegistrosBanRed = banred.Count,
            RegistrosProCredit = procredit.Count,
            SumatoriaBanRed = banred.Sum(t => t.Monto),
            SumatoriaProCredit = procredit.Sum(t => t.Monto)
        };

        Log.Information("BanRed    → Registros: {R}, Sumatoria: {S:C}", resultado.RegistrosBanRed, resultado.SumatoriaBanRed);
        Log.Information("ProCredit → Registros: {R}, Sumatoria: {S:C}", resultado.RegistrosProCredit, resultado.SumatoriaProCredit);

        if (resultado.RegistrosBanRed != resultado.RegistrosProCredit)
        {
            var msg = $"Diferencia en registros: BanRed={resultado.RegistrosBanRed}, ProCredit={resultado.RegistrosProCredit}";
            resultado.Errores.Add(msg);
            Log.Warning(msg);
        }

        if (Math.Abs(resultado.DiferenciaMontos) > 0.01m)
        {
            var msg = $"Diferencia en montos: BanRed={resultado.SumatoriaBanRed:C}, ProCredit={resultado.SumatoriaProCredit:C}";
            resultado.Errores.Add(msg);
            Log.Warning(msg);
        }

        resultado.Conciliado = resultado.Errores.Count == 0;

        Log.Information("Conciliación finalizada. Estado: {Estado}",
            resultado.Conciliado ? "CONCILIADO" : "NO CONCILIADO");

        return resultado;
    }
}
