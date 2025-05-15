import { useState } from "react";
import axios from "axios";
import "../styles/style.css";

const API_URL = `${import.meta.env.VITE_API_URL}/Account`;

function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");

  console.log(email, password);

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(
        `${API_URL}/login`,
        { email, password },
        {
          withCredentials: true, // 🔥 Quan trọng để gửi cookie session đến server
        }
      );

      setMessage(response.data.message || "Đăng nhập thành công");
      console.log("Đăng nhập thành công:", response.data);

      // 👉 Nếu muốn lưu thông tin user vào localStorage:
      localStorage.setItem("userId", response.data.id);
      localStorage.setItem("fullName", response.data.fullName);

      // 👉 Điều hướng qua trang khác nếu cần
      // window.location.href = "/dashboard";
    } catch (error) {
      console.error("Đăng nhập thất bại:", error);
      setMessage(error.response?.data || "Sai tài khoản hoặc mật khẩu");
    }
  };

  return (
    <div className="formLog">
      <form onSubmit={handleLogin}>
        <h2>Đăng nhập</h2>
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <br />
        <input
          type="password"
          placeholder="Mật khẩu"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <br />
        <button type="submit">Đăng nhập</button>
        <p>{message}</p>
      </form>
    </div>
  );
}

export default LoginForm;
