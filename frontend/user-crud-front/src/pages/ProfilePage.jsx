import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import { getProfileRequest, updateUserRequest } from "../api/usersApi";
import { useAuth } from "../auth/AuthContext";
import "../styles/forms.css";

const ProfilePage = () => {
  const { saveSession, token } = useAuth();

  const [profileId, setProfileId] = useState(null);

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
  });

  const [auditInfo, setAuditInfo] = useState({
    role: "",
    isActive: null,
    createdAt: "",
    updatedAt: "",
    createdBy: "",
    updatedBy: "",
  });

  const [loadingProfile, setLoadingProfile] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const normalizeAuditInfo = (user) => {
    return {
      role: user.role || "",
      isActive: user.isActive ?? null,
      createdAt: user.createdAt || "",
      updatedAt: user.updatedAt || "",
      createdBy:
        user.createdBy ||
        user.createdByEmail ||
        user.createdByName ||
        user.createdByUser ||
        "",
      updatedBy:
        user.updatedBy ||
        user.updatedByEmail ||
        user.updatedByName ||
        user.updatedByUser ||
        "",
    };
  };

  const loadProfile = async () => {
    setLoadingProfile(true);
    setError("");
    setSuccess("");

    try {
      const data = await getProfileRequest();
      const user = data.user || data.data || data;

      setProfileId(user.id);

      setForm({
        name: user.name || "",
        email: user.email || "",
        password: "",
      });

      setAuditInfo(normalizeAuditInfo(user));
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo cargar el perfil.";

      setError(message);
    } finally {
      setLoadingProfile(false);
    }
  };

  useEffect(() => {
    loadProfile();
  }, []);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm({
      ...form,
      [name]: value,
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!profileId) {
      setError("No se encontró el usuario actual.");
      return;
    }

    setSaving(true);
    setError("");
    setSuccess("");

    try {
      const payload = {
        name: form.name,
        email: form.email,
      };

      if (form.password.trim() !== "") {
        if (form.password.length < 8) {
          setError("La contraseña debe tener mínimo 8 caracteres.");
          setSaving(false);
          return;
        }

        payload.password = form.password;
      }

      const data = await updateUserRequest(profileId, payload);
      const updatedUser = data.user || data.data || data;

      saveSession({
        token,
        user: updatedUser,
      });

      setForm({
        name: updatedUser.name || "",
        email: updatedUser.email || "",
        password: "",
      });

      setAuditInfo(normalizeAuditInfo(updatedUser));

      setSuccess("Perfil actualizado correctamente.");
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo actualizar el perfil.";

      setError(message);
    } finally {
      setSaving(false);
    }
  };

  const formatDate = (dateValue) => {
    if (!dateValue) return "No disponible";

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return "No disponible";
    }

    return date.toLocaleString("es-CO", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="layout-content">
        <header className="page-header">
          <h1>Mi perfil</h1>
          <p>Consulta, actualiza tus datos personales y revisa la auditoría.</p>
        </header>

        <section className="form-card">
          {loadingProfile ? (
            <p className="form-state">Cargando perfil...</p>
          ) : (
            <>
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
                      placeholder="Tu nombre"
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
                    <label htmlFor="password">Nueva contraseña</label>
                    <input
                      id="password"
                      name="password"
                      type="password"
                      value={form.password}
                      onChange={handleChange}
                      placeholder="Déjala vacía para no cambiarla"
                    />
                  </div>
                </div>

                {error && <p className="form-error">{error}</p>}
                {success && <p className="form-success">{success}</p>}

                <div className="form-actions">
                  <button
                    type="submit"
                    className="primary-button"
                    disabled={saving}
                  >
                    {saving ? "Guardando..." : "Actualizar perfil"}
                  </button>
                </div>
              </form>

              <div className="audit-card">
                <h2>Información de auditoría</h2>

                <div className="audit-grid">
                  <div className="audit-item">
                    <span>Rol</span>
                    <strong>{auditInfo.role || "No disponible"}</strong>
                  </div>

                  <div className="audit-item">
                    <span>Estado</span>
                    <strong>
                      {auditInfo.isActive === null
                        ? "No disponible"
                        : auditInfo.isActive
                        ? "Activo"
                        : "Inactivo"}
                    </strong>
                  </div>

                  <div className="audit-item">
                    <span>Creado el</span>
                    <strong>{formatDate(auditInfo.createdAt)}</strong>
                  </div>

                  <div className="audit-item">
                    <span>Actualizado el</span>
                    <strong>{formatDate(auditInfo.updatedAt)}</strong>
                  </div>

                  <div className="audit-item">
                    <span>Creado por</span>
                    <strong>{auditInfo.createdBy || "No disponible"}</strong>
                  </div>

                  <div className="audit-item">
                    <span>Actualizado por</span>
                    <strong>{auditInfo.updatedBy || "No disponible"}</strong>
                  </div>
                </div>
              </div>
            </>
          )}
        </section>
      </main>
    </div>
  );
};

export default ProfilePage;