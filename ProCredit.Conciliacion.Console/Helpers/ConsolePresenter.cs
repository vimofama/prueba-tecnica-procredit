using Procredit.Conciliacion.Domain;

namespace ProCredit.Conciliacion.Console.Helpers
{
    public static class ConsolePresenter
    {
        public static void MostrarResumen(ResultadoConciliacion resultado)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("--- RESULTADO DE CONCILIACION BANCARIA ---");
            System.Console.WriteLine();

            if (resultado.Conciliado)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("  ESTADO: CONCILIADO");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("  ESTADO: NO CONCILIADO");
            }
            System.Console.ResetColor();

            System.Console.WriteLine();
            System.Console.WriteLine($"  {"CONCEPTO",-22} {"BANRED",15} {"PROCREDIT",15}");
            System.Console.WriteLine($"  {new string('-', 52)}");
            System.Console.WriteLine($"  {"N. Registros",-22} {resultado.RegistrosBanRed,15} {resultado.RegistrosProCredit,15}");
            System.Console.WriteLine($"  {"Sumatoria Montos",-22} {resultado.SumatoriaBanRed,15:N2} {resultado.SumatoriaProCredit,15:N2}");
            System.Console.WriteLine($"  {"Diferencia Registros",-22} {resultado.DiferenciaRegistros,31}");
            System.Console.WriteLine($"  {"Diferencia Montos",-22} {resultado.DiferenciaMontos,31:N2}");
            System.Console.WriteLine();

            if (resultado.Errores.Count > 0)
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("  ERRORES ENCONTRADOS:");
                System.Console.ResetColor();
                foreach (var error in resultado.Errores)
                    System.Console.WriteLine($"  - {error}");
            }

            if (resultado.Advertencias.Count > 0)
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("  ADVERTENCIAS:");
                System.Console.ResetColor();
                foreach (var adv in resultado.Advertencias)
                    System.Console.WriteLine($"  - {adv}");
            }

            System.Console.WriteLine();
        }
    }
}
