# ProCredit – Conciliación Bancaria

Sistema de conciliación bancaria entre archivos de **BanRed** y **ProCredit**.  

---

## Descarga desde GitHub

```bash
git clone https://github.com/vimofama/prueba-tecnica-procredit.git
cd prueba-procredit
```
---

## Formato de Archivos de Entrada

Ambas aplicaciones aceptan archivos `.csv` o `.txt` con delimitador `,` o `;`.

La **primera línea debe ser el header** con exactamente estas 7 columnas en este orden:

```
cuenta origen,cuenta destino,monto,motivo,num identificacion benef,nombre benef,fecha registro
```

Reglas de validación por fila:
- `monto` debe ser un número mayor a `0`.
- `num identificacion benef` debe tener **10 dígitos** (cédula) o **13 dígitos** (RUC).

---

## Ejecución – App de Consola

```bash
cd ProCredit.Conciliacion.Console
```

### Opción A – Archivos por parámetro (recomendado)

```bash
dotnet run -- "<ruta/archivo-banred.txt>" "<ruta/archivo-procredit.txt>"
```

Ejemplo:

```bash
dotnet run -- "C:\archivos\BanRed.txt" "C:\archivos\ProCredit.txt"
```

### Opción B – Archivos configurados en `appsettings.json`

Editar `ProCredit.Conciliacion.Console/appsettings.json`:

```json
{
  "StoragePath": "storage",
  "LogsPath": "logs",
  "Archivos": {
    "BanRed": "storage/BanRed.txt",
    "ProCredit": "storage/ProCredit.txt"
  }
}
```

Luego ejecutar sin parámetros:

```bash
dotnet run
```

### Salida esperada

Si no se encuentran errores

```
--- RESULTADO DE CONCILIACION BANCARIA ---

  ESTADO: CONCILIADO

  CONCEPTO                        BANRED       PROCREDIT
  ----------------------------------------------------
  N. Registros                        19              19
  Sumatoria Montos             17.907,12       17.907,12
  Diferencia Registros                                 0
  Diferencia Montos                                 0,00
```

Si existen errores

```
--- RESULTADO DE CONCILIACION BANCARIA ---

  ESTADO: NO CONCILIADO

  CONCEPTO                        BANRED       PROCREDIT
  ----------------------------------------------------
  N. Registros                         0               0
  Sumatoria Montos                  0,00            0,00
  Diferencia Registros                                 0
  Diferencia Montos                                 0,00


  ERRORES ENCONTRADOS:
  - [BanRed] Columna 1: se esperaba 'cuenta origen', se encontró 'cuenta_origen'        
  - [ProCredit] Fila 17: monto '1555.-02' no es un número válido.
```