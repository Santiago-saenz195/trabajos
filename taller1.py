class Cliente:
    def __init__(self, cedula, nombre, telefono, tipo_cliente, tipo_atencion, cantidad, prioridad, fecha):
        self.cedula = cedula
        self.nombre = nombre
        self.telefono = telefono
        self.tipo_cliente = tipo_cliente.capitalize()
        self.tipo_atencion = tipo_atencion.capitalize()
        self.cantidad = cantidad
        self.prioridad = prioridad.capitalize()
        self.fecha = fecha
        
        self.valor_cita = 0
        self.valor_unidad_atencion = 0
        self.valor_total = 0
        
        self.calcular_costos()

    def calcular_costos(self):
        tarifas_cita = {"Particular": 80000, "Eps": 5000, "Prepagada": 30000}
        self.valor_cita = tarifas_cita.get(self.tipo_cliente, 0)
        if self.tipo_cliente == "Particular":
            precios = {"Limpieza": 60000, "Calzas": 80000, "Extraccion": 100000, "Diagnostico": 50000}
        elif self.tipo_cliente == "Eps":
            precios = {"Limpieza": 0, "Calzas": 40000, "Extraccion": 40000, "Diagnostico": 0}
        else: 
            precios = {"Limpieza": 0, "Calzas": 10000, "Extraccion": 10000, "Diagnostico": 0}

        atencion_limpia = self.tipo_atencion.replace("ó", "o")
        self.valor_unidad_atencion = precios.get(atencion_limpia, 0)
        
        self.valor_total = self.valor_cita + (self.valor_unidad_atencion * self.cantidad)

    def __str__(self):
        return f"Cédula: {self.cedula} | Nombre: {self.nombre} | Pago: ${self.valor_total:,.0f} | Servicio: {self.tipo_atencion}"

def leer_obligatorio(mensaje):
    """Asegura que el campo no sea solo espacios."""
    while True:
        valor = input(mensaje).strip()
        if valor: return valor
        print("⚠️ Error: Este campo no puede quedar vacío.")

def leer_opcion(mensaje, opciones_validas):
    """Asegura que el usuario elija solo opciones de la lista."""
    while True:
        valor = input(mensaje).strip().capitalize()
        valor_busqueda = valor.replace("ó", "o")
        opciones_limpias = [o.capitalize().replace("ó", "o") for o in opciones_validas]
        
        if valor_busqueda in opciones_limpias:
            for original in opciones_validas:
                if original.capitalize().replace("ó", "o") == valor_busqueda:
                    return original
            return valor
        print(f"⚠️ Error: Opción inválida. Elija entre: {', '.join(opciones_validas)}")

def ejecutar_programa():
    lista_clientes = []
    
    print("="*40)
    print("SISTEMA ODONTOLÓGICO - DOCTOR SANTIAGO")
    print("="*40)
    
    while True:
        try:
            n = int(input("¿Cuántos clientes desea registrar? "))
            if n > 0: break
            print("Debe ingresar al menos un cliente.")
        except ValueError:
            print("⚠️ Error: Ingrese un número entero.")

    for i in range(n):
        print(f"\n--- REGISTRO CLIENTE {i+1} ---")
        cedula = leer_obligatorio("Cédula: ")
        nombre = leer_obligatorio("Nombre: ")
        telefono = leer_obligatorio("Teléfono: ")
        
        tipo_c = leer_opcion("Tipo Cliente (Particular, EPS, Prepagada): ", 
                             ["Particular", "EPS", "Prepagada"])
        
        tipo_a = leer_opcion("Tipo Atención (Limpieza, Calzas, Extracción, Diagnóstico): ", 
                             ["Limpieza", "Calzas", "Extracción", "Diagnóstico"])
        
        while True:
            try:
                cant = int(input("Cantidad (Dientes/Servicios): "))
                if (tipo_a in ["Limpieza", "Diagnóstico"] and cant == 1): break
                elif (tipo_a in ["Calzas", "Extracción"] and cant > 0): break
                else:
                    print(f"⚠️ Nota: Para {tipo_a} la cantidad debe ser 1 (según reglas) o mayor a 0.")
                    if tipo_a in ["Limpieza", "Diagnóstico"]: cant = 1; break
            except ValueError:
                print("⚠️ Error: Ingrese un número válido.")
            
        prioridad = leer_opcion("Prioridad (Normal, Urgente): ", ["Normal", "Urgente"])
        fecha = leer_obligatorio("Fecha (DD/MM/AAAA): ")

        lista_clientes.append(Cliente(cedula, nombre, telefono, tipo_c, tipo_a, cant, prioridad, fecha))

    total_ingresos = sum(c.valor_total for c in lista_clientes)
    num_extracciones = sum(1 for c in lista_clientes if c.tipo_atencion == "Extracción")
    
    lista_clientes.sort(key=lambda x: x.valor_total, reverse=True)

    print("\n" + "="*50)
    print("LISTA DE CLIENTES ORDENADA (MAYOR A MENOR PAGO)")
    print("="*50)
    for c in lista_clientes:
        print(c)

    print(f"\nTotal Clientes: {len(lista_clientes)}")
    print(f"Ingresos Totales: ${total_ingresos:,.0f}")
    print(f"Clientes para Extracción: {num_extracciones}")

    print("\n" + "-"*30)
    busqueda = leer_obligatorio("Ingrese Cédula para buscar cliente específico: ")
    encontrado = next((c for c in lista_clientes if c.cedula == busqueda), None)

    if encontrado:
        print(f"✅ CLIENTE ENCONTRADO:\n{encontrado}")
    else:
        print("❌ No se encontró ningún cliente con esa cédula.")

if __name__ == "__main__":
    ejecutar_programa()