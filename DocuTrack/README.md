# DocuTrack – Taller Integrador: Árboles Binarios (MVC en C#)

**Universidad de Manizales – Facultad de Ciencias e Ingeniería – Programación III**

Módulo interno de DocuTrack S.A. que modela su repositorio de documentos (carpetas y archivos) con un
**árbol binario de búsqueda (BST) sin balanceo**, bajo arquitectura **MVC** en una aplicación de consola C#.

## Integrantes y responsabilidades

| Integrante | Responsabilidad |
|---|---|
| Santiago Sáenz Vela | _(p. ej. Model: Nodo y ArbolBinario – CRUD)_ |
| _(Integrante 2)_ | _(p. ej. View: impresión ASCII, búsquedas y métricas)_ |
| _(Integrante 3)_ | _(p. ej. Controller, Program.cs y README)_ |

> Los tres integrantes deben tener commits en el repositorio.

## Requisitos para ejecutar

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior (si usas una versión más nueva, cambia `TargetFramework` en `DocuTrack.csproj`).
- Una terminal con soporte UTF-8 (para ver los conectores `├── └── │`).

```bash
git clone <URL-del-repositorio>
cd DocuTrack
dotnet run
```

No requiere paquetes externos ni configuración adicional.

## Estructura

```
DocuTrack/
├── Program.cs                       Punto de entrada: solo invoca al Controller
├── Models/
│   ├── Nodo.cs                      Nombre, EsCarpeta, Izquierdo, Derecho
│   ├── ArbolBinario.cs              CRUD, recorridos, altura, invariantes
│   └── Resultados.cs                Resultados tipados (sin texto ni Console)
├── Views/
│   └── VistaConsola.cs              Toda la salida: árbol ASCII, resultados, métricas
└── Controllers/
    └── DocumentoController.cs       Secuencia los casos de uso del taller
```

| Capa | Qué hace | Qué NO hace |
|---|---|---|
| Model | Estructura y reglas del BST; devuelve resultados | No usa `Console` |
| View | Imprime árbol, resultados y métricas | No decide ni modifica el árbol |
| Controller | Ordena los casos de uso y llama a Model y View | No contiene lógica estructural del BST |

## Reglas y decisiones de diseño

- **Clave**: `Nombre`, comparación ordinal sin distinguir mayúsculas (`StringComparison.OrdinalIgnoreCase`).
- **Duplicados**: se rechazan (`"DOCUMENTOS"` cuenta como duplicado de `"Documentos"`) y la View registra el intento.
- **Archivo = hoja**: la inserción se rechaza si el nodo quedaría como hijo de un archivo.
- **Actualizar** = `Eliminar(antiguo)` + `Insertar(nuevo)`. Valida que el nuevo nombre no exista y es **atómico**: si la inserción falla, el árbol se restaura al estado previo. El tipo (carpeta/archivo) se conserva.
- **Eliminar con dos hijos**: siempre se usa el **sucesor inorden** (mínimo del subárbol derecho), de forma consistente. Si el sucesor fuera un archivo, la eliminación se rechaza, porque moverlo a un nodo interno dejaría un archivo con hijos. En el conjunto de datos del taller el sucesor siempre es una carpeta.
- **Comparaciones**: se cuenta cada comparación del nombre buscado/insertado contra el nombre de un nodo. La inserción en un árbol vacío cuenta 0.
- **Altura**: número de niveles del camino más largo (árbol vacío = 0). La View también muestra las aristas.

## Casos que ejecuta el programa

1. Construcción con **15 nombres** (carpetas y archivos) + 3 intentos rechazados (duplicado, vacío, hijo de archivo) + árbol ASCII.
2. **6 búsquedas**: 2 en el subárbol izquierdo, 2 en el derecho, 2 inexistentes (con comparaciones).
3. **3 actualizaciones** (hoja, un hijo, raíz) con el árbol impreso tras cada una.
4. **3 eliminaciones** (hoja, un hijo, raíz) con el árbol impreso tras cada una.
5. Recorridos **preorden, inorden, postorden y por niveles** (este último con `Queue`).
6. **Altura** del árbol final.

## ¿Qué recorrido confirma que el BST sigue ordenado?

El **inorden** (izquierdo → nodo → derecho). En un BST todo nombre del subárbol izquierdo es menor que el de su
nodo y todo nombre del derecho es mayor, así que visitar los nodos en inorden produce los nombres en **orden
creciente**. Si después de insertar, actualizar o eliminar el inorden sigue estrictamente creciente, la propiedad del BST se
conservó. El programa lo comprueba automáticamente (`ArbolBinario.EstaOrdenado()`) y lo imprime en la sección 5.

Preorden y postorden no sirven para esto: dependen de la forma del árbol y no salen ordenados.
El recorrido por niveles muestra la estructura por capas, tampoco ordenada.
