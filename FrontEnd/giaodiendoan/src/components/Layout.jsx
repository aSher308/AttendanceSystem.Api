// src/components/Layout.jsx
import React from "react";
import { Link } from "react-router-dom";
import "../styles/style.css";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { API_URL } from "../config";
import { FiLogOut } from "react-icons/fi";
import { Outlet } from "react-router-dom";

const ACCOUNT_API = `${API_URL}/Account`;

export default function Layout() {
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await axios.post(`${ACCOUNT_API}/logout`, {}, { withCredentials: true });
      localStorage.removeItem("userToken"); // hoặc sessionStorage nếu bạn dùng cái đó
      navigate("/login"); // chuyển về trang đăng nhập
    } catch (error) {
      alert("Lỗi khi đăng xuất: " + (error.response?.data || "Không rõ lỗi"));
    }
  };

  return (
    <div className="app-container">
      {/* Header */}
      <header className="app-header">
        <h1 className="app-title">Trang web chấm công online</h1>

        <div
          className="logout-section"
          onClick={handleLogout}
          title="Đăng xuất"
        >
          <FiLogOut size={24} className="logout-icon" />
          <span className="logout-text">Đăng xuất</span>
        </div>
      </header>

      <div className="main-content">
        {/* Sidebar Navigation */}
        <nav className="sidebar">
          <ul className="nav-menu">
            <h5 className="nav-section-title">Menu</h5>
            <li className="nav-item">
              <Link to="home" className="nav-link">
                Trang chủ
              </Link>
            </li>
            <li className="nav-item">
              <Link to="timekeeping" className="nav-link">
                Chấm công
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/report" className="nav-link">
                Nghỉ phép
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/employee" className="nav-link">
                Ca làm việc
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Lịch làm việc
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Báo cáo cá nhân
              </Link>
              <h5 className="nav-section-title">Quản lý</h5>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Quản lý nhân viên
              </Link>
            </li>
            <li className="nav-item">
              <Link to="Shift" className="nav-link">
                Quản lý ca làm
              </Link>
            </li>
            <li className="nav-item">
              <Link to="workSchedule" className="nav-link">
                Quản lý lịch làm
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Đơn nghỉ phép
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Báo cáo tổng hợp
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/settings" className="nav-link">
                Nhật kí hoạt động
              </Link>
            </li>
          </ul>
        </nav>

        {/* Main Content */}
        <main className="app-main">
          <Outlet />
        </main>
      </div>

      {/* Footer */}
      <footer className="app-footer">
        <p>© 2025 Hệ thống chấm công online. All rights reserved.</p>
      </footer>
    </div>
  );
}
