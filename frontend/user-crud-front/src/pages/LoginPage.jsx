import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { loginRequest } from "../api/authApi";
import { useAuth } from "../auth/AuthContext";
import "../styles/auth.css";

const LoginPage = () => {
  const navigate = useNavigate();
  const { saveSession } = useAuth();

  const [form, setForm] = useState({
    email: "admin@demo.com",
    password: "Admin123!",
  });

  const [error, setError] = useState("");
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
    setLoading(true);

    try {
      const data = await loginRequest(form);

      const token = data.accessToken || data.token;

      if (!token) {
        throw new Error("El backend no devolvió un token.");
      }

      const userData = data.user || {
        id: data.id,
        name: data.name,
        email: data.email,
        role: data.role,
      };

      saveSession({
        token,
        user: userData,
      });

      navigate(userData.role === "admin" ? "/users" : "/profile");
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        err.message ||
        "Credenciales inválidas o error al iniciar sesión.";

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-container">
        <div className="auth-card">
          <h1>Iniciar sesión</h1>
          <p>Accede con tu cuenta</p>

          <form className="auth-form" onSubmit={handleSubmit}>
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
                placeholder="********"
                required
              />
            </div>

            {error && <p className="auth-error">{error}</p>}

            <button type="submit" className="primary-button" disabled={loading}>
              {loading ? "Entrando..." : "Entrar"}
            </button>
          </form>

          <Link to="/register" className="auth-link">
            ¿No tienes cuenta? Crear una cuenta
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

export default LoginPage;