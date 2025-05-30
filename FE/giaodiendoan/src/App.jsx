import { Routes, Route } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import Layout from "./components/Layout";
import Home from "./pages/HomePage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import TimekeepingPage from "./pages/TimekeepingPage";
import WorkSchedulePage from "./pages/WorkSchedulePage";
import ShiftPage from "./pages/ShiftPage";
import StatisticsPage from "./pages/StatisticsPage";
import QuanLyNhanVienPage from "./pages/QuanLyNhanVienPage";
import EditUserPage from "./pages/EditUserPage";
import LichNghiPhepCaNhan from "./pages/LichNghiPhepCaNhanPage";
import XemLichLamPage from "./pages/XemLichLamPage";
import QuanLyDonNghiPhepPage from "./pages/QuanLyDonNghiPhepPage";

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
        <Route path="workSchedule" element={<WorkSchedulePage />} />
        <Route path="Shift" element={<ShiftPage />} />
        <Route path="Statistics" element={<StatisticsPage />} />
        <Route path="QuanLyNhanVien" element={<QuanLyNhanVienPage />} />
        <Route path="QuanLyNhanVien/edit/:id" element={<EditUserPage />} />
        <Route path="XemLichLam" element={<XemLichLamPage />} />
        <Route path="LichNghiPhepCaNhan" element={<LichNghiPhepCaNhan />} />
        <Route
          path="QuanLyDonNghiPhepPage"
          element={<QuanLyDonNghiPhepPage />}
        />
      </Route>
    </Routes>
  );
}

export default App;
