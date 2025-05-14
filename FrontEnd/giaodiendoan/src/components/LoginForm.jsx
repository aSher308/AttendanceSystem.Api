import { useState } from "react";
import axios from "axios";
import "../styles/style.css";

const API_URL = `${import.meta.env.VITE_API_URL}/Account`;

function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(
        `${API_URL}/login`,
        { email, password },
        {
          withCredentials: true,
        }
      );
      console.log(response.data);
      setMessage(response.data.message);
      // Bạn có thể lưu user info vào localStorage tại đây nếu cần
    } catch (error) {
      console.log("sai");
      setMessage(error.response?.data || "Đăng nhập thất bại");
    }
  };

  return (
    <div class="formLog">
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
