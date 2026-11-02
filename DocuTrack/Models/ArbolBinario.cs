namespace DocuTrack.Models;

/// <summary>
/// Árbol binario de búsqueda (sin balanceo) que modela el repositorio documental.
/// Clave: Nombre, comparación ordinal sin distinguir mayúsculas. No admite duplicados.
/// Regla de dominio: un archivo siempre es hoja.
/// </summary>
public class ArbolBinario
{
    public Nodo? Raiz { get; private set; }
    public int Cantidad { get; private set; }

    /// <summary>Comparación de claves: ordinal, case-insensitive.</summary>
    public static int Comparar(string a, string b) =>
        string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    // ───────────────────────────── CRUD ─────────────────────────────

    public ResultadoInsercion Insertar(string? nombre, bool esCarpeta)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return new ResultadoInsercion(false, 0, MotivoRechazo.NombreInvalido);

        nombre = nombre.Trim();

        if (Raiz == null)
        {
            Raiz = new Nodo(nombre, esCarpeta);
            Cantidad++;
            return new ResultadoInsercion(true, 0, MotivoRechazo.Ninguno);
        }

        var actual = Raiz;
        int comparaciones = 0;

        while (true)
        {
            comparaciones++;
            int cmp = Comparar(nombre, actual.Nombre);

            if (cmp == 0)
                return new ResultadoInsercion(false, comparaciones, MotivoRechazo.Duplicado);

            var siguiente = cmp < 0 ? actual.Izquierdo : actual.Derecho;
            if (siguiente != null)
            {
                actual = siguiente;
                continue;
            }

            // Punto de inserción hallado: el padre debe ser una carpeta.
            if (!actual.EsCarpeta)
                return new ResultadoInsercion(false, comparaciones, MotivoRechazo.PadreEsArchivo);

            var nuevo = new Nodo(nombre, esCarpeta);
            if (cmp < 0) actual.Izquierdo = nuevo; else actual.Derecho = nuevo;
            Cantidad++;
            return new ResultadoInsercion(true, comparaciones, MotivoRechazo.Ninguno);
        }
    }

    public ResultadoBusqueda Buscar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return new ResultadoBusqueda(null, 0);

        nombre = nombre.Trim();
        var actual = Raiz;
        int comparaciones = 0;

        while (actual != null)
        {
            comparaciones++;
            int cmp = Comparar(nombre, actual.Nombre);
            if (cmp == 0) return new ResultadoBusqueda(actual, comparaciones);
            actual = cmp < 0 ? actual.Izquierdo : actual.Derecho;
        }

        return new ResultadoBusqueda(null, comparaciones);
    }

    /// <summary>
    /// Actualizar = Eliminar(antiguo) + Insertar(nuevo). Es atómico: si algo falla,
    /// el árbol queda exactamente como estaba.
    /// </summary>
    public ResultadoActualizacion Actualizar(string? nombreAntiguo, string? nombreNuevo)
    {
        if (string.IsNullOrWhiteSpace(nombreAntiguo) || string.IsNullOrWhiteSpace(nombreNuevo))
            return new ResultadoActualizacion(false, MotivoRechazo.NombreInvalido, null, null, false);

        var existente = Buscar(nombreAntiguo).Nodo;
        if (existente == null)
            return new ResultadoActualizacion(false, MotivoRechazo.NoExiste, null, null, false);

        if (Buscar(nombreNuevo).Encontrado)
            return new ResultadoActualizacion(false, MotivoRechazo.Duplicado, null, null, false);

        bool esCarpeta = existente.EsCarpeta;
        var respaldoRaiz = Clonar(Raiz);
        int respaldoCantidad = Cantidad;

        var eliminacion = Eliminar(nombreAntiguo);
        if (!eliminacion.Exitoso)
        {
            Restaurar(respaldoRaiz, respaldoCantidad);
            return new ResultadoActualizacion(false, eliminacion.Motivo, eliminacion, null, true);
        }

        var insercion = Insertar(nombreNuevo, esCarpeta);
        if (!insercion.Exitoso)
        {
            Restaurar(respaldoRaiz, respaldoCantidad);
            return new ResultadoActualizacion(false, insercion.Motivo, eliminacion, insercion, true);
        }

        return new ResultadoActualizacion(true, MotivoRechazo.Ninguno, eliminacion, insercion, false);
    }

    /// <summary>
    /// Casos: hoja (desenlazar), un hijo (reconectar hijo con padre) y dos hijos
    /// (reemplazar por el sucesor inorden: mínimo del subárbol derecho).
    /// </summary>
    public ResultadoEliminacion Eliminar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return new ResultadoEliminacion(false, CasoEliminacion.Ninguno, MotivoRechazo.NombreInvalido, null);

        nombre = nombre.Trim();
        Nodo? padre = null;
        var actual = Raiz;

        while (actual != null)
        {
            int cmp = Comparar(nombre, actual.Nombre);
            if (cmp == 0) break;
            padre = actual;
            actual = cmp < 0 ? actual.Izquierdo : actual.Derecho;
        }

        if (actual == null)
            return new ResultadoEliminacion(false, CasoEliminacion.Ninguno, MotivoRechazo.NoExiste, null);

        // Caso 3: dos hijos → el sucesor ocupa el lugar del nodo eliminado.
        if (actual.CantidadHijos == 2)
        {
            var padreSucesor = actual;
            var sucesor = actual.Derecho!;
            while (sucesor.Izquierdo != null)
            {
                padreSucesor = sucesor;
                sucesor = sucesor.Izquierdo;
            }

            // Un archivo no puede quedar como nodo interno.
            if (!sucesor.EsCarpeta)
                return new ResultadoEliminacion(false, CasoEliminacion.DosHijos, MotivoRechazo.SucesorEsArchivo, null);

            // El sucesor no tiene hijo izquierdo: su hijo derecho (si hay) ocupa su lugar.
            if (padreSucesor == actual) actual.Derecho = sucesor.Derecho;
            else padreSucesor.Izquierdo = sucesor.Derecho;

            actual.Nombre = sucesor.Nombre;
            actual.EsCarpeta = sucesor.EsCarpeta;
            Cantidad--;
            return new ResultadoEliminacion(true, CasoEliminacion.DosHijos, MotivoRechazo.Ninguno, sucesor.Nombre);
        }

        // Casos 1 y 2: hoja o un solo hijo.
        var caso = actual.CantidadHijos == 0 ? CasoEliminacion.Hoja : CasoEliminacion.UnHijo;
        var hijo = actual.Izquierdo ?? actual.Derecho;

        if (padre == null) Raiz = hijo;
        else if (padre.Izquierdo == actual) padre.Izquierdo = hijo;
        else padre.Derecho = hijo;

        Cantidad--;
        return new ResultadoEliminacion(true, caso, MotivoRechazo.Ninguno, null);
    }

    // ─────────────────────────── Recorridos ───────────────────────────

    public List<Nodo> Preorden()
    {
        var resultado = new List<Nodo>();
        Preorden(Raiz, resultado);
        return resultado;
    }

    public List<Nodo> Inorden()
    {
        var resultado = new List<Nodo>();
        Inorden(Raiz, resultado);
        return resultado;
    }

    public List<Nodo> Postorden()
    {
        var resultado = new List<Nodo>();
        Postorden(Raiz, resultado);
        return resultado;
    }

    public List<Nodo> PorNiveles()
    {
        var resultado = new List<Nodo>();
        if (Raiz == null) return resultado;

        var cola = new Queue<Nodo>();
        cola.Enqueue(Raiz);
        while (cola.Count > 0)
        {
            var nodo = cola.Dequeue();
            resultado.Add(nodo);
            if (nodo.Izquierdo != null) cola.Enqueue(nodo.Izquierdo);
            if (nodo.Derecho != null) cola.Enqueue(nodo.Derecho);
        }
        return resultado;
    }

    // ───────────────────────────── Métricas ─────────────────────────────

    /// <summary>Altura en niveles (nodos del camino más largo). Árbol vacío = 0.</summary>
    public int Altura() => Altura(Raiz);

    // ─────────────────────────── Invariantes ───────────────────────────

    /// <summary>El inorden debe quedar estrictamente creciente por Nombre.</summary>
    public bool EstaOrdenado()
    {
        var inorden = Inorden();
        for (int i = 1; i < inorden.Count; i++)
            if (Comparar(inorden[i - 1].Nombre, inorden[i].Nombre) >= 0)
                return false;
        return true;
    }

    /// <summary>Ningún archivo tiene hijos.</summary>
    public bool ArchivosSonHojas() => Inorden().All(n => n.EsCarpeta || n.EsHoja);

    // ───────────────────────────── Privados ─────────────────────────────

    private static void Preorden(Nodo? nodo, List<Nodo> destino)
    {
        if (nodo == null) return;
        destino.Add(nodo);
        Preorden(nodo.Izquierdo, destino);
        Preorden(nodo.Derecho, destino);
    }

    private static void Inorden(Nodo? nodo, List<Nodo> destino)
    {
        if (nodo == null) return;
        Inorden(nodo.Izquierdo, destino);
        destino.Add(nodo);
        Inorden(nodo.Derecho, destino);
    }

    private static void Postorden(Nodo? nodo, List<Nodo> destino)
    {
        if (nodo == null) return;
        Postorden(nodo.Izquierdo, destino);
        Postorden(nodo.Derecho, destino);
        destino.Add(nodo);
    }

    private static int Altura(Nodo? nodo) =>
        nodo == null ? 0 : 1 + Math.Max(Altura(nodo.Izquierdo), Altura(nodo.Derecho));

    private static Nodo? Clonar(Nodo? nodo) =>
        nodo == null
            ? null
            : new Nodo(nodo.Nombre, nodo.EsCarpeta)
            {
                Izquierdo = Clonar(nodo.Izquierdo),
                Derecho = Clonar(nodo.Derecho)
            };

    private void Restaurar(Nodo? raiz, int cantidad)
    {
        Raiz = raiz;
        Cantidad = cantidad;
    }
}
