namespace DocuTrack.Models;

/// <summary>
/// Nodo del repositorio documental: una carpeta o un archivo.
/// La clave del BST es <see cref="Nombre"/>.
/// </summary>
public class Nodo
{
    public string Nombre { get; internal set; }
    public bool EsCarpeta { get; internal set; }
    public Nodo? Izquierdo { get; internal set; }
    public Nodo? Derecho { get; internal set; }

    public Nodo(string nombre, bool esCarpeta)
    {
        Nombre = nombre;
        EsCarpeta = esCarpeta;
    }

    public int CantidadHijos => (Izquierdo != null ? 1 : 0) + (Derecho != null ? 1 : 0);

    public bool EsHoja => CantidadHijos == 0;
}
