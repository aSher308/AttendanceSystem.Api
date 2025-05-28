import axios from "axios";

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "https://localhost:7064/api",
  withCredentials: true, // Giữ lại dòng này để gửi cookie session
});

export default axiosInstance;
