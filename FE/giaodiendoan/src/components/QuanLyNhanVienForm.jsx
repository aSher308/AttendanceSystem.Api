import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "../styles/style.css";
import axiosInstance from "../utils/axiosInstance";

function QuanLyNhanVienForm() {
  const navigate = useNavigate();
  const [keyword, setKeyword] = useState("");
  const [department, setDepartment] = useState("");
  const [departments, setDepartments] = useState([]);
  const [users, setUsers] = useState([]);

  // Fetch department list for dropdown
  useEffect(() => {
    axiosInstance
      .get("/Departments")
      .then((res) => setDepartments(res.data))
      .catch((err) => console.error("Failed to load departments:", err));
  }, []);

  // Search users
  const handleSearch = async () => {
    const params = {};
    if (keyword) params.keyword = keyword;
    if (department) params.departmentId = department;

    try {
      const res = await axiosInstance.get("/User", { params });
      setUsers(res.data);
    } catch (err) {
      console.error("Search failed:", err);
    }
  };

  const handleEdit = (id) => {
    navigate(`/layout/QuanLyNhanVien/edit/${id}`);
  };

  const handleDelete = (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa người dùng này?")) {
      axiosInstance
        .delete(`/User/${id}`)
        .then(() => {
          setUsers(users.filter((u) => u.id !== id));
        })
        .catch(() => {
          alert("Xóa thất bại");
        });
    }
  };

  return (
    <div className="quan-ly-nhan-vien-container p-4 max-w-4xl mx-auto">
      <h2 className="text-xl font-bold mb-4">Tìm kiếm người dùng</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
        <input
          type="text"
          placeholder="Nhập tên người dùng"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          className="p-2 border rounded"
        />
        <select
          value={department}
          onChange={(e) => setDepartment(e.target.value)}
          className="p-2 border rounded"
        >
          <option value="">Tất cả phòng ban</option>
          {departments.map((dep) => (
            <option key={dep.id} value={dep.id}>
              {dep.name}
            </option>
          ))}
        </select>
        <button
          onClick={handleSearch}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Tìm kiếm
        </button>
      </div>

      <table className="w-full border text-sm">
        <thead>
          <tr className="bg-gray-100">
            <th className="border p-2">Họ tên</th>
            <th className="border p-2">Email</th>
            <th className="border p-2">Phòng ban</th>
            <th className="border p-2">Vai trò</th>
            <th className="border p-2">Hành động</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td className="border p-2">{user.fullName}</td>
              <td className="border p-2">{user.email}</td>
              <td className="border p-2">{user.departmentName}</td>
              <td className="border p-2">{user.roles?.join(", ")}</td>
              <td className="border p-2 space-x-2">
                <button
                  onClick={() => handleEdit(user.id)}
                  className="text-blue-500 hover:underline"
                >
                  Sửa
                </button>
                <button
                  onClick={() => handleDelete(user.id)}
                  className="text-red-500 hover:underline"
                >
                  Xóa
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default QuanLyNhanVienForm;
