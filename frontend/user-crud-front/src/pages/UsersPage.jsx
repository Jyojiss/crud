import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import { deleteUserRequest, getUsersRequest } from "../api/usersApi";
import "../styles/users.css";

const UsersPage = () => {
  const [users, setUsers] = useState([]);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [size] = useState(10);

  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const loadUsers = async () => {
    setLoading(true);
    setError("");

    try {
      const data = await getUsersRequest({
        search,
        page,
        size,
      });

      const list = data.items || data.users || data.data || data;
      const pages = data.totalPages || data.pages || 1;

      setUsers(Array.isArray(list) ? list : []);
      setTotalPages(pages);
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudieron cargar los usuarios.";

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, [page]);

  const handleSearchSubmit = (event) => {
    event.preventDefault();
    setPage(1);
    loadUsers();
  };

  const handleDelete = async (id) => {
    const confirmDelete = window.confirm(
      "¿Seguro que deseas eliminar este usuario?"
    );

    if (!confirmDelete) return;

    try {
      await deleteUserRequest(id);
      loadUsers();
    } catch (err) {
      const message =
        err.response?.data?.message ||
        err.response?.data?.error ||
        "No se pudo eliminar el usuario.";

      setError(message);
    }
  };

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="layout-content">
        <header className="page-header">
          <h1>Usuarios</h1>
          <p>Busca, pagina y administra usuarios del sistema.</p>
        </header>

        <section className="users-card">
          <div className="users-toolbar">
            <form onSubmit={handleSearchSubmit} className="users-search">
              <input
                type="text"
                placeholder="Buscar por nombre o email..."
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />

              <button type="submit">Buscar</button>
            </form>

            <Link to="/users/new" className="new-user-button">
              + Nuevo usuario
            </Link>
          </div>

          {error && <p className="users-error">{error}</p>}

          {loading ? (
            <p className="users-state">Cargando usuarios...</p>
          ) : (
            <div className="table-wrapper">
              <table className="users-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Email</th>
                    <th>Rol</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>

                <tbody>
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan="5">No hay usuarios para mostrar.</td>
                    </tr>
                  ) : (
                    users.map((user) => (
                      <tr key={user.id}>
                        <td>{user.name}</td>
                        <td>{user.email}</td>
                        <td>
                          <span className="badge badge-role">{user.role}</span>
                        </td>
                        <td>
                          <span
                            className={
                              user.isActive
                                ? "badge badge-active"
                                : "badge badge-inactive"
                            }
                          >
                            {user.isActive ? "Activo" : "Inactivo"}
                          </span>
                        </td>
                        <td>
                          <div className="table-actions">
                            <Link to={`/users/${user.id}/edit`}>Editar</Link>

                            <button
                              type="button"
                              onClick={() => handleDelete(user.id)}
                            >
                              Eliminar
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}

          <div className="pagination">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage(page - 1)}
            >
              Anterior
            </button>

            <span>
              Página {page} de {totalPages}
            </span>

            <button
              type="button"
              disabled={page >= totalPages}
              onClick={() => setPage(page + 1)}
            >
              Siguiente
            </button>
          </div>
        </section>
      </main>
    </div>
  );
};

export default UsersPage;