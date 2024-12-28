# API_Hotels
Este repositorio contiene un proyecto de una API para la gestión de hoteles y reservas. Los servicios han sido desarrollados utilizando Azure Functions con un enfoque en microservicios. Además, la documentación y las pruebas de los endpoints están integradas mediante Swagger.

Funcionalidades
Gestión de Hoteles:

Creación, actualización y desactivación de hoteles.
Búsqueda de hoteles basada en ciudad, fechas y número de huéspedes.
Gestión de Habitaciones:

Creación, actualización y desactivación de habitaciones para hoteles específicos.
Listado de habitaciones por hotel.
Gestión de Reservas:

Creación de reservas para habitaciones específicas.
Visualización de reservas por hotel.
Detalles de una reserva específica.
Gestión de Huéspedes y Contactos de Emergencia:

Asociación de huéspedes a reservas.
Agregar contactos de emergencia para huéspedes.
Documentación
La API está completamente documentada y puede ser probada mediante Swagger. Swagger está configurado y disponible automáticamente cuando la aplicación está en ejecución.

Base de Datos
El respaldo de la base de datos (.bak) se encuentra disponible en el siguiente directorio: API_Hotels/ScriptsSQL/

Configuración
Conexión a la Base de Datos
La cadena de conexión debe ser configurada en el archivo de configuración o en las variables de entorno:
"ConnectionStrings:MyDb": "Server=nombreServer\\SQLEXPRESS;Initial Catalog=pt_hotels;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;"


Ejecución Local
1. Clona este repositorio:
  git clone https://github.com/tu_usuario/API_Hotels.git
  cd API_Hotels
2. Restaura las dependencias:
  dotnet restore
3. Ejecuta la API localmente.
4. Accede a Swagger en:
    http://localhost:7230/api/swagger/ui#/

Autor
Este proyecto fue desarrollado por Cristian Castro Brijaldo. Si tienes dudas o sugerencias, ¡no dudes en contactarme!




