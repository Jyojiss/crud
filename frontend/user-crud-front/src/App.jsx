import { AuthProvider } from "./auth/AuthContext";
import AppRouter from "./routes/AppRouter";

import "./styles/global.css";
import "./styles/layout.css";
import "./styles/users.css";
import "./styles/forms.css";

function App() {
  return (
    <AuthProvider>
      <AppRouter />
    </AuthProvider>
  );
}

export default App;