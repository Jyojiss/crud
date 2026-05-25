import { AuthProvider } from "./auth/AuthContext";
import AppRouter from "./routes/AppRouter";
import "./styles/global.css";

function App() {
  return (
    <AuthProvider>
      <AppRouter />
    </AuthProvider>
  );
}

export default App;