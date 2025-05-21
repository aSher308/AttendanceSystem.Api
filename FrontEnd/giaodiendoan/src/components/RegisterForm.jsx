import { useState } from "react";
import axios from "axios";
import "../styles/style.css";
import { API_URL } from "../config";
import { Link } from "react-router-dom";

const REGISTER_URL = `${API_URL}/Account`;

function RegisterForm() {
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
    phoneNumber: "",
  });

  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleChange = (e) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setSubmitted(true);
    setMessage("");
    setLoading(true);

    if (formData.password !== formData.confirmPassword) {
      setMessage("Mật khẩu không khớp.");
      setLoading(false);
      return;
    }

    const requestData = {
      fullName: formData.fullName,
      email: formData.email,
      password: formData.password,
      phoneNumber: formData.phoneNumber,
    };

    try {
      const response = await axios.post(
        `${REGISTER_URL}/register`,
        requestData
      );
      setMessage(response.data);
    } catch (error) {
      if (error.response?.status === 409) {
        setMessage("Email đã tồn tại.");
      } else {
        setMessage(error.response?.data || "Lỗi đăng ký");
      }
    } finally {
      setLoading(false);
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
      <button type="submit" disabled={loading}>
        {loading ? "Đang đăng ký..." : "Đăng ký"}
      </button>

      {submitted && (
        <p
          style={{
            color: loading
              ? "gray"
              : message.includes("thành công")
              ? "green"
              : "red",
          }}
        >
          {loading ? "Đang đăng ký..." : message}
        </p>
      )}

      <p className="change-password-link">
        <Link to="/">Đăng nhập</Link>
      </p>
    </form>
  );
}

export default RegisterForm;
