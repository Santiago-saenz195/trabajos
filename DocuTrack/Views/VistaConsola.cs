using System.Text;
using DocuTrack.Models;

namespace DocuTrack.Views;

/// <summary>Presentación en consola. No contiene lógica del árbol: solo imprime.</summary>
public class VistaConsola
{
    public VistaConsola()
    {
        // Necesario para que ├── └── │ se vean bien en la terminal.
        Console.OutputEncoding = Encoding.UTF8;
    }

    // ─────────────────────────── Estructura ───────────────────────────

    public void Encabezado(string titulo)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 72));
        Console.WriteLine(titulo);
        Console.WriteLine(new string('=', 72));
    }

    public void Subtitulo(string texto)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {texto} ---");
    }

    public void Mensaje(string texto) => Console.WriteLine(texto);

    // ───────────────────────────── Árbol ASCII ─────────────────────────────

    public void MostrarArbol(ArbolBinario arbol)
    {
        Console.WriteLine();
        if (arbol.Raiz == null)
        {
            Console.WriteLine("(árbol vacío)");
            return;
        }

        Console.WriteLine($"{Etiqueta(arbol.Raiz)}  (raíz)");
        ImprimirHijos(arbol.Raiz, "");
        Console.WriteLine("Leyenda: [C] carpeta, [A] archivo, I: hijo izquierdo, D: hijo derecho");
    }

    private static void ImprimirHijos(Nodo nodo, string prefijo)
    {
        var hijos = new List<(string Lado, Nodo Nodo)>();
        if (nodo.Izquierdo != null) hijos.Add(("I", nodo.Izquierdo));
        if (nodo.Derecho != null) hijos.Add(("D", nodo.Derecho));

        for (int i = 0; i < hijos.Count; i++)
        {
            bool ultimo = i == hijos.Count - 1;
            Console.WriteLine($"{prefijo}{(ultimo ? "└── " : "├── ")}{hijos[i].Lado}: {Etiqueta(hijos[i].Nodo)}");
            ImprimirHijos(hijos[i].Nodo, prefijo + (ultimo ? "    " : "│   "));
        }
    }

    // ───────────────────────────── Operaciones ─────────────────────────────

    public void MostrarInsercion(string nombre, bool esCarpeta, ResultadoInsercion r)
    {
        string tipo = esCarpeta ? "[C]" : "[A]";
        if (r.Exitoso)
            Console.WriteLine($"  [OK]        Insertar {tipo} {nombre}  (comparaciones: {r.Comparaciones})");
        else
            Console.WriteLine($"  [RECHAZADO] Insertar {tipo} \"{nombre}\": {Describir(r.Motivo)}  (comparaciones: {r.Comparaciones})");
    }

    public void MostrarBusqueda(string nombre, string zona, ResultadoBusqueda r)
    {
        string resultado = r.Encontrado ? $"HALLADO -> {Etiqueta(r.Nodo!)}" : "NO HALLADO";
        Console.WriteLine($"  Buscar \"{nombre}\" ({zona}): {resultado}  | comparaciones: {r.Comparaciones}");
    }

    public void MostrarActualizacion(string antiguo, string nuevo, ResultadoActualizacion r)
    {
        Console.WriteLine($"  Actualizar \"{antiguo}\" -> \"{nuevo}\"  (Eliminar + Insertar)");

        if (r.Eliminacion is { Exitoso: true })
            Console.WriteLine($"    1) Eliminar \"{antiguo}\": {DescribirCaso(r.Eliminacion)}");

        if (r.Insercion != null)
            Console.WriteLine($"    2) Insertar \"{nuevo}\": {(r.Insercion.Exitoso ? "OK" : "RECHAZADO - " + Describir(r.Insercion.Motivo))}  (comparaciones: {r.Insercion.Comparaciones})");

        if (r.Exitoso)
            Console.WriteLine("    Resultado: actualización exitosa.");
        else
            Console.WriteLine($"    Resultado: NO se actualizó ({Describir(r.Motivo)}){(r.Revertido ? "; el árbol se restauró" : "")}.");
    }

    public void MostrarEliminacion(string nombre, ResultadoEliminacion r)
    {
        if (r.Exitoso)
            Console.WriteLine($"  Eliminar \"{nombre}\": {DescribirCaso(r)}");
        else
            Console.WriteLine($"  Eliminar \"{nombre}\": NO se eliminó ({Describir(r.Motivo)})");
    }

    // ───────────────────────────── Recorridos y métricas ─────────────────────────────

    public void MostrarRecorrido(string titulo, IEnumerable<Nodo> nodos)
    {
        Console.WriteLine($"  {titulo,-12}: {string.Join(", ", nodos.Select(n => n.Nombre))}");
    }

    public void MostrarVerificacion(bool ordenado, bool archivosHoja)
    {
        Console.WriteLine($"  Inorden estrictamente creciente (BST válido): {(ordenado ? "SÍ" : "NO")}");
        Console.WriteLine($"  Todos los archivos son hojas:                {(archivosHoja ? "SÍ" : "NO")}");
    }

    public void MostrarAltura(int altura, int cantidadNodos)
    {
        Console.WriteLine($"  Nodos en el árbol: {cantidadNodos}");
        Console.WriteLine($"  Altura del árbol:  {altura} niveles ({Math.Max(altura - 1, 0)} aristas en el camino más largo)");
    }

    // ───────────────────────────── Textos ─────────────────────────────

    private static string Etiqueta(Nodo nodo) => $"{(nodo.EsCarpeta ? "[C]" : "[A]")} {nodo.Nombre}";

    private static string DescribirCaso(ResultadoEliminacion r) => r.Caso switch
    {
        CasoEliminacion.Hoja => "caso 1, nodo hoja (se desenlaza del padre)",
        CasoEliminacion.UnHijo => "caso 2, nodo con un hijo (el hijo se reconecta con el padre)",
        CasoEliminacion.DosHijos => $"caso 3, nodo con dos hijos (lo reemplaza su sucesor inorden \"{r.NombreReemplazo}\")",
        _ => "sin cambios"
    };

    private static string Describir(MotivoRechazo motivo) => motivo switch
    {
        MotivoRechazo.NombreInvalido => "el nombre es nulo, vacío o solo espacios",
        MotivoRechazo.Duplicado => "el nombre ya existe (sin distinguir mayúsculas), no se permiten duplicados",
        MotivoRechazo.PadreEsArchivo => "un archivo no puede tener hijos",
        MotivoRechazo.NoExiste => "el nombre no existe en el árbol",
        MotivoRechazo.SucesorEsArchivo => "el sucesor es un archivo y no puede quedar como nodo interno",
        _ => ""
    };
}
