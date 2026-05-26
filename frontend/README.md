# Frontend - user-crud-front

Frontend de la prueba técnica Full Stack Jr para autenticación y CRUD de usuarios.

Construido con **React**, **Vite**, **React Router**, **Axios** y formularios con validación.

---

## Tecnologías

- React
- Vite
- React Router
- Axios
- React Hook Form
- CSS / Tailwind CSS
- LocalStorage para manejo de sesión
- Rutas públicas y privadas

---

## Estructura principal

```bash
frontend/user-crud-front/
├── src/
│   ├── api/
│   │   └── axiosClient.js
│   │
│   ├── auth/
│   │   ├── AuthContext.jsx
│   │   └── PrivateRoute.jsx
│   │
│   ├── components/
│   │   ├── Navbar.jsx
│   │   ├── Loader.jsx
│   │   └── ErrorMessage.jsx
│   │
│   ├── pages/
│   │   ├── LoginPage.jsx
│   │   ├── RegisterPage.jsx
│   │   ├── UsersPage.jsx
│   │   ├── UserFormPage.jsx
│   │   └── ProfilePage.jsx
│   │
│   ├── routes/
│   │   └── AppRoutes.jsx
│   │
│   ├── App.jsx
│   └── main.jsx
│
├── .env
├── package.json
└── README.md
```

---

## Requisitos previos

- Node.js 18+
- npm
- Backend ejecutándose localmente

---

## Instalación

Desde la carpeta del frontend:

```bash
cd frontend/user-crud-front
```

Instalar dependencias:

```bash
npm install
```

---

## Variables de entorno

Crear un archivo `.env` en la raíz del frontend:

```bash
VITE_API_URL=https://localhost:7000/api
```

Ajustar el puerto según la URL real del backend.

Ejemplo:

```bash
VITE_API_URL=http://localhost:5000/api
```

---

## Ejecución

```bash
npm run dev
```

La aplicación normalmente estará disponible en:

```text
http://localhost:5173
```

---

## Scripts disponibles

```bash
npm run dev
```

Ejecuta la aplicación en modo desarrollo.

```bash
npm run build
```

Genera la versión de producción.

```bash
npm run preview
```

Permite previsualizar la versión compilada.

---

## Rutas principales

| Ruta | Tipo | Descripción |
|---|---|---|
| `/login` | Pública | Inicio de sesión |
| `/register` | Pública | Registro de usuario |
| `/users` | Privada admin | Tabla de usuarios |
| `/users/new` | Privada admin | Crear usuario |
| `/users/:id/edit` | Privada admin/dueño | Editar usuario |
| `/profile` | Privada | Perfil del usuario autenticado |

---

## Flujo de autenticación

1. El usuario inicia sesión desde `/login`.
2. El frontend envía las credenciales al backend.
3. El backend retorna un `accessToken` y los datos del usuario.
4. El token se guarda localmente.
5. Axios agrega el token en las peticiones protegidas:

```http
Authorization: Bearer <token>
```

6. Las rutas privadas validan si existe sesión activa.
7. Al cerrar sesión, se elimina el token local y se redirige al login.

---

## Roles en frontend

### Admin

Puede acceder a:

- `/users`
- `/users/new`
- `/users/:id/edit`
- `/profile`

### User

Puede acceder a:

- `/profile`
- edición de su propio perfil

No puede acceder a la administración general de usuarios.

---

## Funcionalidades implementadas

- Login
- Registro
- Logout
- Protección de rutas privadas
- Protección por rol
- Tabla de usuarios
- Búsqueda por nombre o email
- Paginación simple
- Crear usuario
- Editar usuario
- Eliminar usuario
- Ver perfil
- Validaciones de formularios
- Mensajes de error amigables
- Estados de carga
- Consumo de API con Axios

---

## Validaciones sugeridas

### Login

- Email requerido
- Formato de email válido
- Contraseña requerida

### Registro

- Nombre requerido
- Email requerido
- Formato de email válido
- Contraseña requerida
- Contraseña segura

### Usuario

- Nombre requerido
- Email requerido
- Rol válido
- Estado activo/inactivo

---

## Manejo de errores

El frontend debe mostrar mensajes entendibles para casos como:

- Credenciales incorrectas
- Email duplicado
- Token expirado
- Usuario sin permisos
- Backend no disponible
- Error inesperado

Ejemplo:

```text
No tienes permisos para realizar esta acción.
```

```text
No fue posible conectar con el servidor.
```

---

## Accesibilidad y UX

Se recomienda mantener:

- Labels visibles en formularios
- Navegación por teclado
- Estados de focus visibles
- Mensajes de error junto al campo correspondiente
- Botones deshabilitados durante cargas
- Confirmación antes de eliminar usuarios
- Diseño responsive

---

## Configuración con backend

El backend debe tener CORS habilitado para el origen del frontend:

```text
http://localhost:5173
```

Si el frontend no puede conectarse al backend, revisar:

1. Que el backend esté corriendo.
2. Que `VITE_API_URL` tenga el puerto correcto.
3. Que CORS permita el origen del frontend.
4. Que el token JWT esté siendo enviado correctamente.

---

## Credenciales de prueba

| Rol | Email | Contraseña |
|---|---|---|
| Admin | admin@demo.com | Admin123! |
| User | user@demo.com | User123! |

---

## Pruebas manuales sugeridas

1. Entrar a `/login`.
2. Iniciar sesión como admin.
3. Ver la tabla de usuarios.
4. Buscar un usuario por email.
5. Crear un usuario nuevo.
6. Editar el usuario creado.
7. Eliminarlo.
8. Cerrar sesión.
9. Iniciar sesión como user.
10. Verificar que no pueda entrar a `/users`.
11. Editar el perfil propio.

---

## Problemas comunes

### El frontend no consume la API

Revisar `.env`:

```bash
VITE_API_URL=https://localhost:7000/api
```

Reiniciar Vite después de modificar `.env`:

```bash
npm run dev
```

### Error de CORS

Verificar que el backend permita:

```text
http://localhost:5173
```

### Token no llega al backend

Revisar que Axios esté enviando:

```http
Authorization: Bearer <token>
```

### Las rutas privadas no funcionan

Verificar que el contexto de autenticación esté envolviendo la aplicación y que el token se guarde correctamente.

---

## Build de producción

```bash
npm run build
```

Los archivos generados quedarán en:

```bash
dist/
```

---

## Estado

El frontend cumple el alcance mínimo de la prueba técnica:

- Login
- Registro
- Rutas protegidas
- Consumo de API
- CRUD de usuarios
- Perfil de usuario
- Manejo de roles
- Búsqueda y paginación
- Validaciones básicas
- Mensajes de error y carga
