import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import {
  createUserRequest,
  getUserByIdRequest,
  updateUserRequest,
} from "../api/usersApi";
import "../styles/forms.css";

const UserFormPage = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const isEditMode = Boolean(id);

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
    role: "user",
    isActive: true,
  });

  const [loading, setLoading] = useState(false);
  const [loadingUser, setLoadingUser] = useState(false);
  const [error, setError] = useState("");

  const loadUser = async () => {
    if (!isEditMode) return;

    setLoadingUser(true);
    setError("");

    try {
      const data = await getUserByIdRequest(id);

      const user = data.user || data.data || data;

      setForm({
        name: user.name || "",
        email: user.email || "",
        password: "",
        role: user.role || "user",
        isActive: user.isActive ?? true,
      });
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo cargar el usuario.";

      setError(message);
    } finally {
      setLoadingUser(false);
    }
  };

  useEffect(() => {
    loadUser();
  }, [id]);

  const handleChange = (event) => {
    const { name, value, type, checked } = event.target;

    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value,
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      const payload = {
        name: form.name,
        email: form.email,
        role: form.role,
        isActive: form.isActive,
      };

      if (!isEditMode || form.password.trim() !== "") {
        payload.password = form.password;
      }

      if (!isEditMode && form.password.length < 8) {
        setError("La contraseña debe tener mínimo 8 caracteres.");
        setLoading(false);
        return;
      }

      if (isEditMode) {
        console.log("Payload enviado:", payload);
        await updateUserRequest(id, payload);
      } else {
        await createUserRequest(payload);
      }

      navigate("/users");
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo guardar el usuario.";

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="layout-content">
        <header className="page-header">
          <h1>{isEditMode ? "Editar usuario" : "Crear usuario"}</h1>
          <p>
            {isEditMode
              ? "Actualiza los datos del usuario seleccionado."
              : "Registra un nuevo usuario en el sistema."}
          </p>
        </header>

        <section className="form-card">
          {loadingUser ? (
            <p className="form-state">Cargando usuario...</p>
          ) : (
            <form className="user-form" onSubmit={handleSubmit}>
              <div className="form-grid">
                <div className="form-group">
                  <label htmlFor="name">Nombre</label>
                  <input
                    id="name"
                    name="name"
                    type="text"
                    value={form.name}
                    onChange={handleChange}
                    placeholder="Ej: Ana Rodríguez"
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="email">Email</label>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    value={form.email}
                    onChange={handleChange}
                    placeholder="correo@demo.com"
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="password">
                    {isEditMode ? "Nueva contraseña" : "Contraseña"}
                  </label>
                  <input
                    id="password"
                    name="password"
                    type="password"
                    value={form.password}
                    onChange={handleChange}
                    placeholder={
                      isEditMode
                        ? "Déjala vacía para no cambiarla"
                        : "Mínimo 8 caracteres"
                    }
                    required={!isEditMode}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="role">Rol</label>
                  <select
                    id="role"
                    name="role"
                    value={form.role}
                    onChange={handleChange}
                    required
                  >
                    <option value="user">Usuario</option>
                    <option value="admin">Administrador</option>
                  </select>
                </div>
              </div>

              <label className="checkbox-field">
                <input
                  type="checkbox"
                  name="isActive"
                  checked={form.isActive}
                  onChange={handleChange}
                />
                Usuario activo
              </label>

              {error && <p className="form-error">{error}</p>}

              <div className="form-actions">
                <button type="submit" className="primary-button" disabled={loading}>
                  {loading
                    ? "Guardando..."
                    : isEditMode
                    ? "Actualizar"
                    : "Crear usuario"}
                </button>

                <button
                  type="button"
                  className="secondary-button"
                  onClick={() => navigate("/users")}
                >
                  Cancelar
                </button>
              </div>
            </form>
          )}
        </section>
      </main>
    </div>
  );
};

export default UserFormPage;