# Restaurante Web

Aplicación ASP.NET Core MVC para administrar productos, clientes, empleados, mesas,
pedidos y facturas. Usa Entity Framework Core con SQL Server.

## Requisitos

- SDK de **.NET 10** (el proyecto tiene como destino `net10.0`).
- **SQL Server** accesible desde el equipo donde se ejecuta la aplicación.
  Para desarrollo puede usarse SQL Server Developer o Express.
- Una cuenta con acceso a la base. Para crearla y aplicar las migraciones también
  necesita permisos de creación de bases y modificación del esquema.
- Git para clonar el repositorio.

## Preparar y compilar

```powershell
git clone https://github.com/brandonquiroga908-netizen/Restaurante-Web.git
cd Restaurante-Web
dotnet --list-sdks
dotnet restore
dotnet build
```

## Configurar la conexión con variables de entorno

Ejecuta los siguientes comandos en PowerShell, desde la raíz del repositorio.
Las variables deben estar definidas en la misma terminal que ejecutará la aplicación.
ASP.NET Core convierte `__` en `:` y estas variables tienen prioridad sobre
`appsettings.json`, que contiene el nombre de un equipo de desarrollo.

Ejemplo con autenticación integrada de Windows y una base local de pruebas:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=RestaurantePruebas;Trusted_Connection=True;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Facturacion__TasaImpuesto = "0"
```

Sustituye `localhost` por tu servidor o instancia, por ejemplo `localhost\SQLEXPRESS`.
Si usas autenticación SQL, configura la misma variable con una cadena como esta,
sustituyendo el usuario y la contraseña por tus credenciales:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=RestaurantePruebas;User Id=usuario;Password=TU_CLAVE;Encrypt=True;TrustServerCertificate=True"
```

No guardes credenciales reales en archivos versionados. `TrustServerCertificate=True`
es una opción para desarrollo local; en un despliegue usa un certificado válido y
configura la conexión según el servidor.

`Facturacion__TasaImpuesto` es una proporción: `0` significa sin impuesto y `0.13`
representa un 13 % como ejemplo de configuración. Define la tasa que corresponda
a tu instalación. No se pueden emitir facturas si no hay una tasa configurada.

## Crear la base e iniciar la aplicación

Para la primera ejecución sobre una base de pruebas nueva:

```powershell
$env:Inicializacion__AplicarMigracionesYDatosPrueba = "true"
dotnet run --project Restaurante/Restaurante.csproj --no-launch-profile --urls http://localhost:5049
```

La inicialización aplica las migraciones existentes y genera clientes de prueba
si la tabla de clientes está vacía. No crea mesas, categorías ni productos.
Abre <http://localhost:5049>. Si la conexión falla, comprueba que SQL Server esté
iniciado, que el servidor o instancia sea correcto y que la cuenta tenga permisos.

Detén la aplicación con `Ctrl+C`. Para las siguientes ejecuciones, cuando la base
ya esté preparada:

```powershell
$env:Inicializacion__AplicarMigracionesYDatosPrueba = "false"
dotnet run --project Restaurante/Restaurante.csproj --no-launch-profile --urls http://localhost:5049
```

La inicialización está desactivada de forma predeterminada. Actívala de manera
controlada cuando necesites aplicar migraciones; utiliza una base de pruebas para
las comprobaciones siguientes.

## Comprobar el flujo pedido → mesa → factura

1. Crea una categoría, un producto activo con precio, una mesa **Libre** y un
   cliente (puedes usar uno de los clientes de prueba).
2. Crea un pedido con esa mesa, cliente y al menos un producto. Debe quedar
   **Pendiente** y la mesa debe pasar a **Ocupada**.
3. Desde Mesas, intenta cambiar esa mesa a **Libre** o **Reservada**, tanto al
   editarla como con el selector rápido. Ambas operaciones deben mostrar un error
   y conservarla **Ocupada**. Un valor de estado fuera del catálogo también debe
   rechazarse al crear, editar o cambiar el estado de una mesa.
4. En el detalle del pedido pendiente no debe aparecer **Ver / generar factura**.
   Abre `/Facturas/Create?pedidoId=ID` o `/Facturas/PorPedido/ID`, sustituyendo `ID`
   por el número del pedido: debe indicarse que solo se facturan pedidos
   entregados. El pedido tampoco debe aparecer en el selector de Nueva factura.
   Un envío directo al formulario de emisión debe rechazar el estado igualmente.
5. Pasa el pedido a **En preparación**. La mesa sigue **Ocupada**, no se permite
   volver a Pendiente y continúa bloqueada la facturación y la liberación o
   reserva manual de la mesa.
6. Marca el pedido como **Entregado**. La mesa queda **Libre** si no tiene otro
   pedido Pendiente o En preparación. Ahora aparece el botón de factura y el
   pedido se ofrece en Nueva factura.
7. Genera la factura eligiendo un método de pago. Comprueba subtotal, impuesto y
   total. La factura debe quedar **Pendiente** y debe poder consultarse desde
   Facturas o desde el pedido. Intentar facturarlo otra vez debe rechazarse.
8. Intenta cancelar o eliminar el pedido facturado. Debe conservarse el pedido y
   mostrarse un error. Esto también se aplica si su factura fue anulada. Un
   pedido Entregado o Cancelado no debe poder reabrirse.
9. Con otro pedido sin factura, comprueba que cancelarlo desde Pendiente o
   En preparación libera la mesa si no quedan otros pedidos activos.
10. Si tienes datos históricos con una factura emitida mientras el pedido estaba
    Pendiente, comprueba que puede avanzar a En preparación y después Entregado,
    liberando la mesa al finalizar. No debe poder cancelarse ni eliminarse, y su
    factura original debe conservarse.

Las transiciones admitidas son:

| Estado actual | Destinos permitidos |
| --- | --- |
| Pendiente | En preparación, Entregado o Cancelado |
| En preparación | Entregado o Cancelado |
| Entregado / Cancelado | Ninguno |

La cancelación requiere que el pedido no tenga ninguna factura. Se mantiene la
entrega directa desde Pendiente que ofrece la vista. Los controles del servidor
también se aplican a solicitudes manipuladas y al guardar una factura desde un
formulario que llevaba tiempo abierto.

`dotnet build` comprueba la compilación. Las comprobaciones del flujo requieren
una instancia SQL Server y datos de prueba; compilar por sí solo no verifica la
conexión ni las operaciones sobre la base.
