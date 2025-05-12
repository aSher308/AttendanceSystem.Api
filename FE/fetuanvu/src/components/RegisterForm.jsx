import { useState } from "react";
import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL;

function RegisterForm() {
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [message, setMessage] = useState("");

  const handleChange = (e) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(`${API_URL}/register`, formData, {
        withCredentials: true,
      });
      setMessage(response.data);
    } catch (error) {
      setMessage(error.response?.data || "Lỗi đăng ký");
    }
  };

  return (
    <form onSubmit={handleRegister}>
      <h2>Đăng ký</h2>
      <input
        name="fullName"
        placeholder="Họ tên"
        onChange={handleChange}
        required
      />
      <br />
      <input
        name="email"
        type="email"
        placeholder="Email"
        onChange={handleChange}
        required
      />
      <br />
      <input
        name="password"
        type="password"
        placeholder="Mật khẩu"
        onChange={handleChange}
        required
      />
      <br />
      <input
        name="confirmPassword"
        type="password"
        placeholder="Xác nhận mật khẩu"
        onChange={handleChange}
        required
      />
      <br />
      <button type="submit">Đăng ký</button>
      <p>{message}</p>
    </form>
  );
}

export default RegisterForm;
