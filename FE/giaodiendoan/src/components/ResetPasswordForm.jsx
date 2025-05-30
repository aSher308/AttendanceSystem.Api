import React, { useState } from "react";
import { API_URL } from "../config";
import { Link } from "react-router-dom";
import "../styles/style.css";

function ResetPasswordForm() {
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState("");
  const [status, setStatus] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (newPassword !== confirmPassword) {
      setStatus("error");
      setMessage("Mật khẩu xác nhận không khớp.");
      return;
    }

    try {
      const response = await fetch(`${API_URL}/Account/reset-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          token,
          newPassword,
        }),
      });

      const result = await response.text();
      if (response.ok) {
        setStatus("success");
        setMessage(result);
      } else {
        setStatus("error");
        setMessage(result);
      }
    } catch (error) {
      console.log(error);
      setStatus("error");
      setMessage("Lỗi kết nối đến server.");
    }
  };

  return (
    <div className="layout-form-container">
      <div className="formLog">
        <form onSubmit={handleSubmit}>
          <h3>Đặt lại mật khẩu</h3>

          <div className="form-group mb-3">
            <label>Mã token (gửi qua email):</label>
            <input
              type="text"
              className="form-control"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              required
            />
          </div>

          <div className="form-group mb-3">
            <label>Mật khẩu mới:</label>
            <input
              type="password"
              className="form-control"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>

          <div className="form-group mb-3">
            <label>Nhập lại mật khẩu mới:</label>
            <input
              type="password"
              className="form-control"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="btn btn-success">
            Xác nhận
          </button>
          <p className="change-password-link">
            <Link to="/">Đăng nhập</Link>
          </p>

          {message && (
            <div
              className={`alert mt-3 alert-${
                status === "success" ? "success" : "danger"
              }`}
            >
              {message}
            </div>
          )}
        </form>
      </div>
    </div>
  );
}

export default ResetPasswordForm;
