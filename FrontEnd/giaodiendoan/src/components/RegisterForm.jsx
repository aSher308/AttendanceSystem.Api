import { useState } from "react";
import axios from "axios";
import "../styles/style.css";
import { API_URL } from "../config";
import { Link } from "react-router-dom";

const REGISTER_URL = `${API_URL}/Account`; // chữ thường, đúng với controller route

function RegisterForm() {
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
    phoneNumber: "",
  });

  const [message, setMessage] = useState("");

  const handleChange = (e) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleRegister = async (e) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      setMessage("Mật khẩu không khớp.");
      return;
    }

    // Chuẩn bị dữ liệu gửi lên backend
    const requestData = {
      fullName: formData.fullName,
      email: formData.email,
      password: formData.password,
      phoneNumber: formData.phoneNumber,
    };

    try {
      const response = await axios.post(
        `${REGISTER_URL}/register`, // -> https://localhost:5001/api/account/register
        requestData
      );
      setMessage(response.data);
    } catch (error) {
      // Nếu lỗi 409 (Conflict) do email tồn tại sẽ được backend trả về
      if (error.response?.status === 409) {
        setMessage("Email đã tồn tại.");
      } else {
        setMessage(error.response?.data || "Lỗi đăng ký");
      }
    }
  };

  return (
    <form className="formLog" onSubmit={handleRegister}>
      <h2>Đăng ký</h2>
      <input
        name="fullName"
        placeholder="Họ tên"
        onChange={handleChange}
        value={formData.fullName}
        required
      />
      <input
        name="email"
        type="email"
        placeholder="Email"
        onChange={handleChange}
        value={formData.email}
        required
      />
      <input
        name="phoneNumber"
        type="tel"
        placeholder="Số điện thoại"
        onChange={handleChange}
        value={formData.phoneNumber}
        required
      />
      <input
        name="password"
        type="password"
        placeholder="Mật khẩu"
        onChange={handleChange}
        value={formData.password}
        required
      />
      <input
        name="confirmPassword"
        type="password"
        placeholder="Xác nhận mật khẩu"
        onChange={handleChange}
        value={formData.confirmPassword}
        required
      />
      <button type="submit">Đăng ký</button>
      <p className="change-password-link">
        <Link to="/">Đăng nhập</Link>
      </p>
      <p>{message}</p>
    </form>
  );
}

export default RegisterForm;
