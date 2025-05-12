import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5000/api/account", // Thay port nếu khác
  withCredentials: true, // Đảm bảo cookie-based session hoạt động
});

export default api;
