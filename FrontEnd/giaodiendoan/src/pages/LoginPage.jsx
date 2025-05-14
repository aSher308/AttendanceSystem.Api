import LoginForm from "../components/LoginForm";
import { Link } from "react-router-dom";
import "../styles/style.css";

function LoginPage() {
  return (
    <div>
      <h1>Đây là trang đăng nhập</h1>
      <LoginForm />
      <p>
        <Link to="/register">Bấm vào đây để đăng ký tài khoản</Link>
      </p>
    </div>
  );
}

export default LoginPage;
