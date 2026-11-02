namespace DocuTrack.Models;

// El Model no imprime nada: devuelve resultados tipados y la View decide cómo mostrarlos.

public enum MotivoRechazo
{
    Ninguno,
    NombreInvalido,    // nulo, vacío o solo espacios
    Duplicado,         // el nombre ya existe (sin distinguir mayúsculas)
    PadreEsArchivo,    // el nodo quedaría como hijo de un archivo
    NoExiste,          // el nombre a eliminar/actualizar no está en el árbol
    SucesorEsArchivo   // mover el sucesor a un nodo interno dejaría un archivo con hijos
}

public enum CasoEliminacion
{
    Ninguno,
    Hoja,
    UnHijo,
    DosHijos
}

public record ResultadoInsercion(bool Exitoso, int Comparaciones, MotivoRechazo Motivo);

public record ResultadoBusqueda(Nodo? Nodo, int Comparaciones)
{
    public bool Encontrado => Nodo != null;
}

/// <param name="NombreReemplazo">Nombre del sucesor que ocupó el lugar (solo en el caso de dos hijos).</param>
public record ResultadoEliminacion(bool Exitoso, CasoEliminacion Caso, MotivoRechazo Motivo, string? NombreReemplazo);

/// <param name="Revertido">true si la inserción falló y el árbol se restauró a su estado previo.</param>
public record ResultadoActualizacion(
    bool Exitoso,
    MotivoRechazo Motivo,
    ResultadoEliminacion? Eliminacion,
    ResultadoInsercion? Insercion,
    bool Revertido);
