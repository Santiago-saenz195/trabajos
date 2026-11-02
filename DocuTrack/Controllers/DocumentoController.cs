using DocuTrack.Models;
using DocuTrack.Views;

namespace DocuTrack.Controllers;

/// <summary>
/// Orquesta los casos de uso del taller en el orden pedido.
/// No contiene lógica estructural del BST: delega todo en <see cref="ArbolBinario"/>
/// y delega toda la impresión en <see cref="VistaConsola"/>.
/// </summary>
public class DocumentoController
{
    private readonly ArbolBinario _arbol;
    private readonly VistaConsola _vista;

    // Orden de inserción que produce: raíz con dos subárboles, hojas a cada lado,
    // nodos con un solo hijo (Actas, Anexos) y más de 14 nombres.
    private static readonly (string Nombre, bool EsCarpeta)[] DatosIniciales =
    {
        ("Documentos", true),
        ("Contratos", true),
        ("Reportes", true),
        ("Actas", true),
        ("Cotizaciones", true),
        ("Facturas", true),
        ("Ventas", true),
        ("Anexos", true),
        ("Balance.xlsx", false),
        ("Correo.txt", false),
        ("Cronogramas", true),
        ("Estadisticas", true),
        ("Inventario.docx", false),
        ("Tesoreria.pdf", false),
        ("Zona.txt", false)
    };

    public DocumentoController() : this(new ArbolBinario(), new VistaConsola()) { }

    public DocumentoController(ArbolBinario arbol, VistaConsola vista)
    {
        _arbol = arbol;
        _vista = vista;
    }

    public void Ejecutar()
    {
        ConstruirArbol();
        EjecutarBusquedas();
        EjecutarActualizaciones();
        EjecutarEliminaciones();
        MostrarRecorridos();
        MostrarMetricasFinales();
    }

    // ───────────────────────── 1. Construcción ─────────────────────────

    private void ConstruirArbol()
    {
        _vista.Encabezado("1. CONSTRUCCIÓN DEL ÁRBOL");

        foreach (var (nombre, esCarpeta) in DatosIniciales)
            _vista.MostrarInsercion(nombre, esCarpeta, _arbol.Insertar(nombre, esCarpeta));

        _vista.Subtitulo("Validaciones: intentos que deben ser rechazados");
        IntentarInsertar("DOCUMENTOS", true);     // duplicado con distinta capitalización
        IntentarInsertar("   ", true);            // nombre vacío
        IntentarInsertar("Balance2.txt", false);  // quedaría como hijo de un archivo

        _vista.Subtitulo("Árbol inicial");
        _vista.MostrarArbol(_arbol);
    }

    private void IntentarInsertar(string nombre, bool esCarpeta) =>
        _vista.MostrarInsercion(nombre, esCarpeta, _arbol.Insertar(nombre, esCarpeta));

    // ───────────────────────── 2. Búsquedas ─────────────────────────

    private void EjecutarBusquedas()
    {
        _vista.Encabezado("2. BÚSQUEDAS RÁPIDAS (6)");

        (string Nombre, string Zona)[] busquedas =
        {
            ("actas", "existente, subárbol izquierdo"),
            ("Correo.txt", "existente, subárbol izquierdo"),
            ("Facturas", "existente, subárbol derecho"),
            ("TESORERIA.PDF", "existente, subárbol derecho"),
            ("Contabilidad", "inexistente"),
            ("Nomina.xlsx", "inexistente")
        };

        foreach (var (nombre, zona) in busquedas)
            _vista.MostrarBusqueda(nombre, zona, _arbol.Buscar(nombre));
    }

    // ───────────────────────── 3. Actualizaciones ─────────────────────────

    private void EjecutarActualizaciones()
    {
        _vista.Encabezado("3. ACTUALIZACIONES SELECTIVAS (Eliminar + Insertar)");

        Actualizar("3.1 Actualizar una HOJA (archivo)", "Correo.txt", "Correspondencia.txt");
        Actualizar("3.2 Actualizar un nodo con UN HIJO", "Actas", "Actividades");
        Actualizar("3.3 Actualizar la RAÍZ", "Documentos", "Expedientes");
    }

    private void Actualizar(string titulo, string antiguo, string nuevo)
    {
        _vista.Subtitulo(titulo);
        _vista.MostrarActualizacion(antiguo, nuevo, _arbol.Actualizar(antiguo, nuevo));
        _vista.MostrarArbol(_arbol);
    }

    // ───────────────────────── 4. Eliminaciones ─────────────────────────

    private void EjecutarEliminaciones()
    {
        _vista.Encabezado("4. ELIMINACIONES SELECTIVAS");

        Eliminar("4.1 Eliminar una HOJA", "Zona.txt");
        // Tras 4.1, Ventas quedó con un único hijo (Tesoreria.pdf).
        Eliminar("4.2 Eliminar un nodo con UN HIJO (Ventas quedó solo con Tesoreria.pdf)", "Ventas");
        // La raíz actual tiene dos hijos: se reemplaza por su sucesor inorden.
        Eliminar("4.3 Eliminar la RAÍZ (dos hijos, sucesor inorden)", _arbol.Raiz?.Nombre ?? string.Empty);
    }

    private void Eliminar(string titulo, string nombre)
    {
        _vista.Subtitulo(titulo);
        _vista.MostrarEliminacion(nombre, _arbol.Eliminar(nombre));
        _vista.MostrarArbol(_arbol);
    }

    // ───────────────────────── 5. Recorridos ─────────────────────────

    private void MostrarRecorridos()
    {
        _vista.Encabezado("5. RECORRIDOS DE VERIFICACIÓN");

        _vista.MostrarRecorrido("Preorden", _arbol.Preorden());
        _vista.MostrarRecorrido("Inorden", _arbol.Inorden());
        _vista.MostrarRecorrido("Postorden", _arbol.Postorden());
        _vista.MostrarRecorrido("Por niveles", _arbol.PorNiveles());

        _vista.Subtitulo("Verificación (el inorden debe salir ordenado alfabéticamente)");
        _vista.MostrarVerificacion(_arbol.EstaOrdenado(), _arbol.ArchivosSonHojas());
    }

    // ───────────────────────── 6. Métricas ─────────────────────────

    private void MostrarMetricasFinales()
    {
        _vista.Encabezado("6. MÉTRICAS FINALES");
        _vista.MostrarAltura(_arbol.Altura(), _arbol.Cantidad);
        _vista.Mensaje("");
    }
}
