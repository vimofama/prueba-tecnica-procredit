using Procredit.Conciliacion.Domain;
using Serilog;

namespace Procedrit.Conciliacion.Application.Services;

public class ArchivoService
{
    private readonly string _storagePath;

    public ArchivoService(string storagePath)
    {
        _storagePath = storagePath;
    }

    public void ValidarExtension(string rutaArchivo)
    {
        Log.Information("Validando extensión del archivo: {Archivo}", rutaArchivo);

        var extension = Path.GetExtension(rutaArchivo).ToLower();

        if (extension != ".csv" && extension != ".txt")
        {
            Log.Error("Extensión inválida: {Extension}. Solo se permiten .csv y .txt", extension);
            throw new InvalidOperationException($"Formato inválido: '{extension}'. El archivo debe ser .csv o .txt");
        }

        Log.Information("Extensión valida: {Extension}", extension);
    }

    public List<Transaccion> LeerArchivo(string rutaArchivo, string nombreArchivo, out List<string> erroresLectura)
    {
        erroresLectura = new List<string>();
        Log.Information("Iniciando lectura del archivo: {Archivo}", rutaArchivo);

        if (!File.Exists(rutaArchivo))
        {
            Log.Error("Archivo no encontrado: {Archivo}", rutaArchivo);
            throw new FileNotFoundException($"No se encontró el archivo: {rutaArchivo}");
        }

        var lineas = File.ReadAllLines(rutaArchivo);

        if (lineas.Length == 0)
        {
            Log.Error("El archivo está vacío: {Archivo}", rutaArchivo);
            throw new InvalidOperationException("El archivo está vacío.");
        }

        var delimitador = lineas[0].Contains(';') ? ';' : ',';
        Log.Information("Delimitador detectado: '{Delimitador}'", delimitador);

        var transacciones = new List<Transaccion>();

        for (int i = 1; i < lineas.Length; i++)
        {
            var linea = lineas[i].Trim();
            if (string.IsNullOrWhiteSpace(linea)) continue;

            var columnas = linea.Split(delimitador);

            if (columnas.Length != 7)
            {
                var error = $"[{nombreArchivo}] Fila {i + 1}: tiene {columnas.Length} columnas (se esperan 7).";
                erroresLectura.Add(error);
                Log.Warning("{Error}. Se omite.", error);
                continue;
            }

            if (!decimal.TryParse(columnas[2].Trim(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal monto))
            {
                var error = $"[{nombreArchivo}] Fila {i + 1}: monto '{columnas[2].Trim()}' no es un número válido.";
                erroresLectura.Add(error);
                Log.Warning("{Error} Se omite.", error);
                continue;
            }

            transacciones.Add(new Transaccion
            {
                CuentaOrigen = columnas[0].Trim(),
                CuentaDestino = columnas[1].Trim(),
                Monto = monto,
                Motivo = columnas[3].Trim(),
                NumIdentificacion = columnas[4].Trim(),
                NombreBeneficiario = columnas[5].Trim(),
                FechaRegistro = columnas[6].Trim()
            });
        }

        Log.Information("Lectura completada: {Total} transacciones cargadas", transacciones.Count);
        return transacciones;
    }

    public void GuardarCopia(string rutaArchivo, string nombreLogico)
    {
        var fechaDir = DateTime.Now.ToString("yyyy-MM-dd");
        var timestamp = DateTime.Now.ToString("HHmmss");
        var extension = Path.GetExtension(rutaArchivo);
        var dirDestino = Path.Combine(_storagePath, fechaDir);

        Directory.CreateDirectory(dirDestino);

        var nombreDestino = $"{nombreLogico}_{timestamp}{extension}";
        var rutaDestino = Path.Combine(dirDestino, nombreDestino);

        File.Copy(rutaArchivo, rutaDestino, overwrite: true);

        Log.Information("Copia guardada en: {Ruta}", rutaDestino);
    }

    public string[] ObtenerHeader(string rutaArchivo)
    {
        var primeraLinea = File.ReadLines(rutaArchivo).First();
        var delimitador = primeraLinea.Contains(';') ? ';' : ',';
        return primeraLinea.Split(delimitador).Select(c => c.Trim().ToLower()).ToArray();
    }

}
