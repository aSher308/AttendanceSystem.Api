import React, { useState } from "react";
import { API_URL } from "../config";
import "../styles/style.css";
import { Link } from "react-router-dom";

function ForgotPasswordForm() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [status, setStatus] = useState(null); // success / error

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch(`${API_URL}/Account/forgot-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(email),
      });

      if (response.ok) {
        const result = await response.text();
        setStatus("success");
        setMessage(result);
      } else {
        const error = await response.text();
        setStatus("error");
        setMessage(error);
      }
    } catch (error) {
      console.error(error);
      setStatus("error");
      setMessage("Lỗi kết nối đến server.");
    }
  };

  return (
    <div className="layout-form-container">
      <div className="formLog">
        <form onSubmit={handleSubmit}>
          <h3>Quên mật khẩu</h3> <br />
          <div className="form-group mb-3">
            <label>Nhập email để khôi phục mật khẩu:</label>
            <br />
            <input
              type="email"
              className="form-control"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary">
            Gửi liên kết đặt lại mật khẩu
          </button>
          <p className="change-password">
            <Link to="/reset-password">Đổi mật khẩu</Link> <br /> <br />
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

export default ForgotPasswordForm;
