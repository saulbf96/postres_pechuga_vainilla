# Pechuga y Vainilla · documento del proyecto

## 1. El negocio

Dos emprendimientos bajo una misma marca y una misma página:

| | Vainilla | Pechuga |
|---|---|---|
| Qué vende | Postres: fresas con crema, panquesitos y más | Salado: burritos, cuernitos, chapatas y más |
| Quién | Yo (administrador) | Mi hermana (vendedora) |
| Entrega | A domicilio en mi zona, recoger en casa, más adelante en el local | Punto en la UAM entre semana y en la UNAM los sábados |
| Pago | Tarjeta, SPEI y OXXO con pasarela, más efectivo o transferencia | Efectivo o transferencia; tarjeta en línea posible (el dinero llega a mi cuenta y hacemos corte) |

- Nombre: **Pechuga y Vainilla** (Pechuga es un gatito; Vainilla, una perrita). Frase: *Dulce con Vainilla, salado con Pechuga.*
- Tech Link Solutions construye el sitio y aparece en el pie de página como "Sitio creado por Tech Link Solutions".
- Zona: [tu alcaldía], colonia [colonia]. **La dirección completa nunca se muestra en público**, solo se da al confirmar el pedido.
- Se promociona también en grupos de Facebook de la universidad (revisar reglas de la universidad y de cada grupo).

## 2. Stack y hosting

- **Frontend:** Angular (SPA, sin SSR al inicio) + Tailwind CSS. Después PWA y, si crece, Ionic + Capacitor.
- **Backend:** ASP.NET Core Web API (.NET 10 LTS) con Entity Framework Core.
- **Base de datos:** SQL Server (LocalDB o Express en desarrollo).
- **Login:** ASP.NET Core Identity. Roles: `Administrador`, `Vendedor`, `Cliente`, `Soporte`.
- **Pagos:** una pasarela (Mercado Pago o Stripe) detrás de la interfaz `IPaymentProvider`, más efectivo y transferencia.
- **Hosting:** MonsterASP.NET. Plan gratis para desarrollar (1 sitio, 1 base de datos de 1 GB, subdominio gratuito) y plan Premium con dominio propio antes de vender de verdad. Confirmar precios y límites vigentes al registrarse.
- **Cómo se despliega:** `ng build` genera archivos estáticos que se copian a `wwwroot` de la API. La API sirve el sitio y las rutas `/api/...`
  (`UseDefaultFiles`, `UseStaticFiles`, `MapFallbackToFile("index.html")`). Un solo sitio, sin CORS entre front y API.
- **Fotos** como archivos en `wwwroot/imagenes` (5 GB disponibles), comprimidas. Solo la ruta va en la base de datos.
- **API versionada** desde el inicio: `/api/v1/...`, para que la futura app no se rompa si la API cambia.

## 3. Estructura de la solución

```
PechugaVainilla/
├─ CLAUDE.md
├─ docs/PROYECTO.md
├─ src/
│  ├─ PechugaVainilla.Api/            controllers, Program.cs, wwwroot (Angular compilado)
│  ├─ PechugaVainilla.Core/           entidades, enums, interfaces (IPaymentProvider)
│  └─ PechugaVainilla.Infrastructure/ EF Core, migraciones, pasarelas de pago
├─ tests/PechugaVainilla.Tests/       xUnit
└─ client/                            proyecto Angular (código fuente)
```

## 4. Reglas del negocio

1. Un **checkout** puede tener productos de los dos vendedores. Se divide en **un pedido por vendedor** (mismo `CheckoutId`), cada uno con su total, entrega, pago y estado.
2. Cada vendedor tiene sus **puntos de entrega**. Cada punto tiene días de la semana, horario, **hora límite para pedir** (ejemplo: hasta las 8 pm del día anterior) y costo de envío.
3. La página solo deja elegir puntos, días y horas válidos para **todo** el carrito. Si un producto no se entrega en el punto elegido, avisa antes de confirmar.
4. **Pago y entrega son estados distintos** en cada pedido.
   - Estado del pedido: `Nuevo`, `Preparando`, `Listo`, `Entregado`, `Cancelado`.
   - Estado del pago: `Pendiente`, `Pagado`, `PorConfirmar` (transferencia), `PorCobrar` (efectivo al entregar), `Cobrado`.
5. El precio se **copia al detalle del pedido** en el momento de la compra (`PedidoDetalle.PrecioUnitario`), para que un cambio de precio no altere pedidos anteriores.
6. **Cantidad máxima por día** por producto, para no pasarse (los postres con crema son perecederos).
7. **Sin cuenta:** el cliente puede pedir por WhatsApp con un mensaje prellenado (`https://wa.me/52NUMERO?text=...`, texto codificado). **Con cuenta:** ve su historial y el seguimiento.
8. **Entrega cruzada (etapa posterior):** mi hermana puede entregar postres de Vainilla en su punto. El punto indica qué vendedor **entrega** y de qué vendedores **acepta** productos.
9. **Cortes:** los pagos en línea llegan a mi cuenta de la pasarela; el panel calcula cuánto le toca a cada vendedor y qué efectivo tiene cada quien.
10. Los vendedores solo ven y modifican **sus** productos, puntos y pedidos. El administrador ve todo.

## 5. Modelo de datos (versión inicial)

Enums guardados como texto para que se lean en la base. Dinero en `decimal(10,2)`. Fechas en UTC.

- **Vendedor:** `Id`, `Nombre`, `Slug`, `WhatsApp`, `Descripcion`, `Activo`.
- **Producto:** `Id`, `VendedorId`, `Nombre`, `Descripcion`, `Alergenos`, `FotoRuta`, `MaxPorDia`, `Activo`.
- **Presentacion:** `Id`, `ProductoId`, `Nombre` (Chico, Mediano, Grande, Pieza), `Precio`, `Activo`.
- **PuntoEntrega:** `Id`, `VendedorId`, `Nombre`, `Tipo` (`Domicilio`, `Recoger`, `Campus`), `DiasSemana` (enum `[Flags]`), `HoraInicio`, `HoraFin`, `DiasAnticipacion`, `HoraLimitePedido`, `CostoEnvio`, `Activo`.
- **Usuario** (Identity): `Id`, `Nombre`, `WhatsApp`, `Email`, y su rol. Un `Vendedor` se relaciona con un usuario.
- **Pedido:** `Id`, `CheckoutId`, `UsuarioId` (opcional), `NombreCliente`, `WhatsApp`, `VendedorId`, `PuntoEntregaId`, `FechaEntrega`, `HoraEntrega`, `DetalleEntrega` (dirección, facultad o salón), `Total`, `Estado`, `CreadoEn`.
- **PedidoDetalle:** `Id`, `PedidoId`, `ProductoId`, `PresentacionId`, `NombreProducto`, `PrecioUnitario`, `Cantidad`, `Notas`.
- **Pago:** `Id`, `PedidoId`, `Metodo` (`Tarjeta`, `SPEI`, `OXXO`, `Efectivo`, `Transferencia`), `Estado`, `Monto`, `ReferenciaProveedor`, `PagadoEn`, `CobradoPorVendedorId`.

Más adelante: `PuntoEntregaAcepta` (para la entrega cruzada) y `Corte`.

## 6. Etapas

1. **Catálogo:** API con vendedores y productos, base de datos con datos de ejemplo, Angular mostrando el catálogo con filtros y el botón "Pedir por WhatsApp".
2. **Cuentas y pedidos:** registro, login, carrito, confirmación y seguimiento.
3. **Panel:** pedidos, productos, puntos de entrega.
4. **Pagos:** pasarela, webhooks, estados de pago y cortes.
5. **Pulido:** PWA, SEO local (prerender de páginas públicas), chatbot.

## 7. Diseño

Estilo: pastelería fina con un toque tierno. Nada de aspecto de plantilla: sin componentes por defecto, con los colores y el logo propios.

- **Colores:** crema `#FBF5EA`, rojo fresa `#8F2A3F`, cacao `#3B2A26`, verde hoja `#3F5E3A`, rosa suave `#F3DDD6`, verde suave `#E3EADC`, texto secundario `#6E5A52`, borde `#D8C7B3`.
- **Tipografías:** Fraunces (títulos) y DM Sans (texto), de Google Fonts.
- **Logo:** sello circular con Vainilla (perrita crema de orejas caídas) y Pechuga (gatito naranja atigrado). Versiones: sello, horizontal, ícono y stickers.
- **Pantallas ya diseñadas:** Login, Inicio, Catálogo (computadora y celular), Detalle de producto, Carrito, Confirmar pedido, Seguimiento, Crear cuenta, Mis pedidos, Historia y contacto, y el panel (Pedidos, Detalle de pedido, Productos, Puntos de entrega, Cortes).
- Los mockups están en el lienzo de diseño de Claude (enlace privado mío). Claude Code no puede abrirlo: si necesita ver una pantalla, se la describo o le paso una captura.

## 8. Datos pendientes de llenar

Alcaldía y colonia, horarios, número de WhatsApp, precios, descripciones, alérgenos, fotos de productos y de las mascotas, horarios de mi hermana en cada campus, y titular de la cuenta de la pasarela.

## 9. Pendientes fuera del código

- Comprobar que `pechugayvainilla` esté libre (Instagram, Facebook, dominio) y revisar el buscador de marcas del IMPI.
- Acordar con mi hermana quién es dueña de la marca y cómo se manejan los postres que ella entrega y el efectivo.
- Confirmar si la universidad permite vender dentro del campus.
- Aviso de privacidad y términos de venta (que los revise un abogado) y orientación fiscal con un contador.
- Crear el Perfil de Negocio de Google.
