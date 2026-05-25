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

  const [loadingProfile, setLoadingProfile] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const loadProfile = async () => {
    setLoadingProfile(true);
    setError("");

    try {
      const data = await getProfileRequest();
      const user = data.user || data.data || data;

      setProfileId(user.id);

      setForm({
        name: user.name || "",
        email: user.email || "",
        password: "",
      });
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

      const updatedUser = await updateUserRequest(profileId, payload);
      const user = updatedUser.user || updatedUser.data || updatedUser;

      saveSession({
        token,
        user,
      });

      setForm({
        name: user.name || "",
        email: user.email || "",
        password: "",
      });

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

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="layout-content">
        <header className="page-header">
          <h1>Mi perfil</h1>
          <p>Consulta y actualiza tus datos personales.</p>
        </header>

        <section className="form-card">
          {loadingProfile ? (
            <p className="form-state">Cargando perfil...</p>
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
                <button type="submit" className="primary-button" disabled={saving}>
                  {saving ? "Guardando..." : "Actualizar perfil"}
                </button>
              </div>
            </form>
          )}
        </section>
      </main>
    </div>
  );
};

export default ProfilePage;