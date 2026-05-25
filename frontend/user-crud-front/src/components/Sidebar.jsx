import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const Sidebar = () => {
  const navigate = useNavigate();
  const { logout, user, isAdmin } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <aside className="sidebar">
      <div>
        <h2 className="sidebar-logo">UserAdmin</h2>
        <p className="sidebar-user">{user?.email}</p>

        <nav className="sidebar-nav">
          {isAdmin && (
            <>
              <NavLink to="/users">Usuarios</NavLink>
              <NavLink to="/users/new">Crear usuario</NavLink>
            </>
          )}

          <NavLink to="/profile">Perfil</NavLink>

          <button type="button" onClick={handleLogout}>
            Cerrar sesión
          </button>
        </nav>
      </div>
    </aside>
  );
};

export default Sidebar;