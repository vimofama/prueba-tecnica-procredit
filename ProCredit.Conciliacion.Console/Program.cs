using Microsoft.Extensions.Configuration;
using Procredit.Conciliacion.Domain;
using ProCredit.Conciliacion.Console.Helpers;
using Procedrit.Conciliacion.Application.Services;
using Serilog;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var storagePath = config["StoragePath"] ?? "storage";
var logsPath = config["LogsPath"] ?? "logs";
var rutaBanRed = args.Length > 0 ? args[0] : config["Archivos:BanRed"] ?? "";
var rutaProCredit = args.Length > 1 ? args[1] : config["Archivos:ProCredit"] ?? "";

Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(logsPath, "conciliacion_.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var archivoService = new ArchivoService(storagePath);
var validacionService = new ValidacionService();
var conciliacionService = new ConciliacionService();

var resultado = new ResultadoConciliacion();

try
{
    Log.Information("Inicio proceso de conciliacion bancaria");

    archivoService.ValidarExtension(rutaBanRed);
    archivoService.ValidarExtension(rutaProCredit);

    var headerBanRed = archivoService.ObtenerHeader(rutaBanRed);
    var headerProCredit = archivoService.ObtenerHeader(rutaProCredit);

    var erroresHeaderBanRed = validacionService.ValidarEstructuraHeader(headerBanRed);
    var erroresHeaderProCredit = validacionService.ValidarEstructuraHeader(headerProCredit);

    resultado.Errores.AddRange(erroresHeaderBanRed.Select(e => $"[BanRed] {e}"));
    resultado.Errores.AddRange(erroresHeaderProCredit.Select(e => $"[ProCredit] {e}"));

    var headerBanRedValido = erroresHeaderBanRed.Count == 0;
    var headerProCreditValido = erroresHeaderProCredit.Count == 0;

    var transaccionesBanRed = new List<Transaccion>();
    var transaccionesProCredit = new List<Transaccion>();

    if (headerBanRedValido)
    {
        transaccionesBanRed = archivoService.LeerArchivo(rutaBanRed, "BanRed", out var erroresLecturaBanRed);
        resultado.Errores.AddRange(erroresLecturaBanRed);
        archivoService.GuardarCopia(rutaBanRed, "banred");

        var erroresCamposBanRed = validacionService.ValidarTransacciones(transaccionesBanRed, "BanRed");
        resultado.Errores.AddRange(erroresCamposBanRed);
    }
    else
    {
        Log.Warning("Se omite la validación de filas para BanRed por header inválido.");
    }

    if (headerProCreditValido)
    {
        transaccionesProCredit = archivoService.LeerArchivo(rutaProCredit, "ProCredit", out var erroresLecturaProCredit);
        resultado.Errores.AddRange(erroresLecturaProCredit);
        archivoService.GuardarCopia(rutaProCredit, "procredit");

        var erroresCamposProCredit = validacionService.ValidarTransacciones(transaccionesProCredit, "ProCredit");
        resultado.Errores.AddRange(erroresCamposProCredit);
    }
    else
    {
        Log.Warning("Se omite la validación de filas para ProCredit por header inválido.");
    }

    if (!headerBanRedValido || !headerProCreditValido)
    {
        Log.Error("Errores en la estructura del header. Se omite la conciliación final.");
        return;
    }

    var resultadoConciliacion = conciliacionService.Conciliar(transaccionesBanRed, transaccionesProCredit);

    resultado.RegistrosBanRed = resultadoConciliacion.RegistrosBanRed;
    resultado.RegistrosProCredit = resultadoConciliacion.RegistrosProCredit;
    resultado.SumatoriaBanRed = resultadoConciliacion.SumatoriaBanRed;
    resultado.SumatoriaProCredit = resultadoConciliacion.SumatoriaProCredit;
    resultado.Errores.AddRange(resultadoConciliacion.Errores);
    resultado.Conciliado = resultado.Errores.Count == 0;

    Log.Information("Proceso finalizado");
}
catch (FileNotFoundException ex)
{
    Log.Error("Archivo no encontrado: {Mensaje}", ex.Message);
    resultado.Errores.Add($"Archivo no encontrado: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Log.Error("Error de validación: {Mensaje}", ex.Message);
    resultado.Errores.Add($"Error de validación: {ex.Message}");
}
catch (Exception ex)
{
    Log.Fatal("Error inesperado: {Mensaje}", ex.Message);
    resultado.Errores.Add($"Error inesperado: {ex.Message}");
}
finally
{
    ConsolePresenter.MostrarResumen(resultado);
    Log.CloseAndFlush();
}