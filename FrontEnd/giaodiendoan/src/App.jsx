import { Routes, Route } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import Layout from "./components/Layout";
import Home from "./pages/HomePage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import TimekeepingPage from "./pages/TimekeepingPage";

function App() {
  return (
    <Routes>
      {/* Các route không dùng layout */}
      <Route path="/" element={<LoginPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      {/* Route dùng layout */}
      <Route path="/layout" element={<Layout />}>
        <Route index element={<Home />} /> {/* Trang mặc định */}
        <Route path="home" element={<Home />} />
        <Route path="timekeeping" element={<TimekeepingPage />} />
      </Route>
    </Routes>
  );
}

export default App;
