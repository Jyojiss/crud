import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registerRequest } from "../api/authApi";
import "../styles/auth.css";

const RegisterPage = () => {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm({
      ...form,
      [name]: value,
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (form.password.length < 8) {
      setError("La contraseña debe tener mínimo 8 caracteres.");
      return;
    }

    setLoading(true);

    try {
      await registerRequest(form);

      setSuccess("Usuario registrado correctamente.");

      setTimeout(() => {
        navigate("/login");
      }, 900);
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo registrar el usuario.";

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-container">
        <div className="auth-card auth-card-register">
          <h1>Crear cuenta</h1>
          <p>Registra un nuevo usuario para acceder al sistema.</p>

          <form className="auth-form" onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="name">Nombre</label>
              <input
                id="name"
                name="name"
                type="text"
                value={form.name}
                onChange={handleChange}
                placeholder="Ej: Jorge Carvajal"
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
              <label htmlFor="password">Contraseña</label>
              <input
                id="password"
                name="password"
                type="password"
                value={form.password}
                onChange={handleChange}
                placeholder="Mínimo 8 caracteres"
                required
              />
            </div>

            {error && <p className="auth-error">{error}</p>}
            {success && <p className="auth-success">{success}</p>}

            <button type="submit" className="primary-button" disabled={loading}>
              {loading ? "Creando..." : "Crear cuenta"}
            </button>
          </form>

          <Link to="/login" className="auth-link">
            ¿Ya tienes cuenta? Iniciar sesión
          </Link>
        </div>
      </section>

      <footer className="auth-footer">
        <div>
          <h2>UserAdmin CRUD</h2>
          <p>
            Gestión segura de usuarios con roles, autenticación JWT y
            operaciones CRUD.
          </p>
        </div>

        <div>
          <p>React + Vite · ASP.NET Core · SQL Server</p>
          <p>Desarrollado por Jorge Carvajal · © 2026</p>
        </div>
      </footer>
    </main>
  );
};

export default RegisterPage;