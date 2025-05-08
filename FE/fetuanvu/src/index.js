// src/index.js
import React from "react";
import ReactDOM from "react-dom";
import LoginPage from "./pages/LoginPage";
import "./style/style.css"; // Đảm bảo bạn đã tạo file CSS ở đây

ReactDOM.render(
  <React.StrictMode>
    <LoginPage />
  </React.StrictMode>,
  document.getElementById("root")
);
