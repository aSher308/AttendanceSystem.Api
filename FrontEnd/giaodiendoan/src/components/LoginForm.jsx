import { useState } from "react";
import axios from "axios";
import "../styles/style.css";
import { useNavigate } from "react-router-dom";
import { API_URL } from "../config";
import { Link } from "react-router-dom";

const ACCOUNT_API = `${API_URL}/Account`;

function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate(); // 👈 Khởi tạo navigate hook

  // console.log(email, password);

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(
        `${ACCOUNT_API}/login`,
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

      // 👉 Điều hướng đến trang layout (hoặc trang bạn muốn)
      navigate("/layout"); // hoặc "/dashboard" tùy route của bạn
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
        <p className="register-link">
          <Link to="/register">Bấm vào đây để đăng ký tài khoản</Link>
        </p>
        <p className="forgot-password-link">
          <Link to="/forgot-password">Quên mật khẩu?</Link>
        </p>
      </form>
    </div>
  );
}

export default LoginForm;
