using Procredit.Conciliacion.Domain;
using Serilog;

namespace Procedrit.Conciliacion.Application.Services;

public class ValidacionService
{
    public List<string> ValidarEstructuraHeader(string[] headerReal)
    {
        Log.Information("Validando estructura del header...");
        var errores = new List<string>();
        var esperadas = Transaccion.ColumnasEsperadas;

        if (headerReal.Length != esperadas.Length)
        {
            errores.Add($"Se esperan {esperadas.Length} columnas, se encontraron {headerReal.Length}.");
            Log.Warning("Número de columnas incorrecto: esperadas={Esp}, encontradas={Real}",
                esperadas.Length, headerReal.Length);
            return errores;
        }

        for (int i = 0; i < esperadas.Length; i++)
        {
            if (headerReal[i] != esperadas[i])
            {
                var error = $"Columna {i + 1}: se esperaba '{esperadas[i]}', se encontró '{headerReal[i]}'";
                errores.Add(error);
                Log.Warning(error);
            }
        }

        if (errores.Count == 0)
            Log.Information("Header válido: todas las columnas son correctas");

        return errores;
    }

    public List<string> ValidarTransacciones(List<Transaccion> transacciones, string nombreArchivo)
    {
        Log.Information("Validando {Total} transacciones de {Archivo}...", transacciones.Count, nombreArchivo);
        var errores = new List<string>();

        for (int i = 0; i < transacciones.Count; i++)
        {
            var t = transacciones[i];
            var fila = i + 2;

            if (!t.IdentificacionValida)
            {
                var error = $"[{nombreArchivo}] Fila {fila}: num_identificacion '{t.NumIdentificacion}' " +
                            $"tiene longitud {t.NumIdentificacion.Length} (válido: 10 o 13 dígitos)";
                errores.Add(error);
                Log.Warning(error);
            }

            if (!t.MontoValido)
            {
                var error = $"[{nombreArchivo}] Fila {fila}: monto '{t.Monto}' debe ser mayor a 0";
                errores.Add(error);
                Log.Warning(error);
            }
        }

        if (errores.Count == 0)
            Log.Information("Todas las transacciones de {Archivo} son válidas", nombreArchivo);

        return errores;
    }
}
