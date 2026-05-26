# Backend - UserCrudApi

Backend de la prueba técnica Full Stack Jr para autenticación y CRUD de usuarios.

Construido con **ASP.NET Core Web API**, **.NET 8**, **Entity Framework Core**, **SQL Server**, **JWT** y **BCrypt**.

---

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- BCrypt.Net
- Swagger / OpenAPI
- CORS

---

## Estructura principal

```bash
backend/UserCrudApi/
├── Controllers/
│   ├── AuthController.cs
│   └── UsersController.cs
│
├── Data/
│   ├── AppDbContext.cs
│   └── SeedData.cs
│
├── DTOs/
│   ├── Auth/
│   └── Users/
│
├── Models/
│   └── User.cs
│
├── Services/
│   ├── TokenService.cs
│   └── PasswordService.cs
│
├── Migrations/
├── Program.cs
├── appsettings.json
└── UserCrudApi.csproj
```

---

## Requisitos previos

- .NET SDK 8
- SQL Server local o SQL Server en Docker
- Herramienta de Entity Framework Core:

```bash
dotnet tool install --global dotnet-ef
```

Si ya está instalada:

```bash
dotnet tool update --global dotnet-ef
```

Verificar instalación:

```bash
dotnet ef --version
```

---

## Configuración de SQL Server con Docker

```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Admin12345!" \
  -p 1433:1433 \
  --name sqlserver-crud \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Verificar:

```bash
docker ps
```

---

## Variables sensibles

No se recomienda dejar la cadena de conexión ni la clave JWT directamente en el repositorio.

Configurar `user-secrets`:

```bash
cd backend/UserCrudApi

dotnet user-secrets init
```

Agregar cadena de conexión:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=UserCrudDb;User Id=sa;Password=Admin12345!;TrustServerCertificate=True;"
```

Agregar clave JWT:

```bash
dotnet user-secrets set "Jwt:Key" "clave-super-secreta-de-desarrollo-con-mas-de-32-caracteres"
```

Ejemplo de configuración esperada:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=UserCrudDb;User Id=sa;Password=Admin12345!;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "clave-super-secreta-de-desarrollo-con-mas-de-32-caracteres",
    "Issuer": "UserCrudApi",
    "Audience": "UserCrudClient",
    "ExpireMinutes": 60
  }
}
```

---

## Instalación

Desde la carpeta del backend:

```bash
cd backend/UserCrudApi
```

Restaurar paquetes:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

---

## Migraciones

Crear una migración nueva:

```bash
dotnet ef migrations add NombreDeLaMigracion
```

Aplicar migraciones:

```bash
dotnet ef database update
```

La base de datos se creará automáticamente si no existe.

---

## Ejecución

```bash
dotnet run
```

El backend quedará disponible en la URL indicada por la consola, por ejemplo:

```text
https://localhost:7000
http://localhost:5000
```

Swagger:

```text
https://localhost:7000/swagger
```

---

## Swagger con JWT

Para probar endpoints protegidos:

1. Ejecutar `/api/auth/login`.
2. Copiar el `accessToken`.
3. Presionar el botón **Authorize** en Swagger.
4. Ingresar:

```text
Bearer <accessToken>
```

5. Probar los endpoints protegidos.

---

## Credenciales de prueba

Si el seed fue ejecutado correctamente:

| Rol | Email | Contraseña |
|---|---|---|
| Admin | admin@demo.com | Admin123! |
| User | user@demo.com | User123! |

---

## Endpoints

### Auth

#### Registrar usuario

```http
POST /api/auth/register
```

Body:

```json
{
  "email": "nuevo@demo.com",
  "password": "Nuevo123!",
  "name": "Usuario Nuevo"
}
```

#### Login

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "admin@demo.com",
  "password": "Admin123!"
}
```

Respuesta esperada:

```json
{
  "accessToken": "jwt-token",
  "user": {
    "id": "uuid",
    "email": "admin@demo.com",
    "name": "Administrador",
    "role": "admin"
  }
}
```

#### Logout

```http
POST /api/auth/logout
```

Este endpoint puede revocar el refresh token si la funcionalidad está habilitada.

---

### Users

#### Listar usuarios

```http
GET /api/users?search=&page=1&size=10
```

Rol requerido: `admin`.

#### Ver usuario por ID

```http
GET /api/users/{id}
```

Permitido para:

- `admin`
- dueño del perfil

#### Crear usuario

```http
POST /api/users
```

Rol requerido: `admin`.

Body:

```json
{
  "email": "created@demo.com",
  "password": "Created123!",
  "name": "Usuario Creado",
  "role": "user",
  "isActive": true
}
```

#### Actualizar usuario

```http
PUT /api/users/{id}
```

Permitido para:

- `admin`
- dueño del perfil, con restricciones

Body:

```json
{
  "name": "Nombre Actualizado",
  "email": "actualizado@demo.com",
  "role": "user",
  "isActive": true
}
```

#### Eliminar usuario

```http
DELETE /api/users/{id}
```

Rol requerido: `admin`.

---

## Reglas de autorización

### Admin

Puede administrar todos los usuarios.

### User

Puede consultar y editar únicamente su propio perfil.

Si un usuario intenta modificar otro perfil, la API debe responder con `403 Forbidden`.

---

## Seguridad implementada

- Contraseñas cifradas con BCrypt.
- No se guardan contraseñas en texto plano.
- JWT con expiración.
- Endpoints protegidos con `[Authorize]`.
- Validación de roles.
- DTOs de respuesta sin campos sensibles.
- Validación de email único.
- CORS configurado para permitir el frontend local.

---

## Auditoría

El modelo puede incluir campos de auditoría como:

- `CreatedAt`
- `UpdatedAt`
- `CreatedBy`
- `UpdatedBy`

Estos campos permiten identificar cuándo y por quién fue creado o modificado un registro.

---

## Errores comunes

### `IDX10653: The encryption algorithm 'HS256' requires a key size...`

La clave JWT es demasiado corta.

Solución: usar una clave de mínimo 32 caracteres.

```bash
dotnet user-secrets set "Jwt:Key" "clave-super-secreta-de-desarrollo-con-mas-de-32-caracteres"
```

### `dotnet ef` no se reconoce

Agregar las herramientas globales de .NET al PATH o reinstalar:

```bash
dotnet tool update --global dotnet-ef
```

### No se crea la tabla después de crear la migración

Crear la migración no aplica cambios en la base de datos.

Ejecutar:

```bash
dotnet ef database update
```

---

## Comandos útiles

```bash
dotnet restore
dotnet build
dotnet run
dotnet ef migrations add NombreMigracion
dotnet ef database update
dotnet ef migrations list
```

---

## Estado

El backend cumple el alcance mínimo de la prueba técnica:

- Registro
- Login
- JWT
- CRUD
- Roles
- SQL Server
- Swagger
- CORS
- Hash de contraseña
- Búsqueda y paginación
- Validación de permisos
- Auditoría básica
