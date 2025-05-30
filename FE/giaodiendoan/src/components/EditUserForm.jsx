import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axiosInstance from "../utils/axiosInstance";

function EditUserForm() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [user, setUser] = useState(null);
  const [departments, setDepartments] = useState([]);
  const [roles, setRoles] = useState([]);

  const [formData, setFormData] = useState({
    id: id,
    fullName: "",
    email: "",
    phoneNumber: "",
    departmentId: "",
    roles: [], // Thêm roles vào formData
    isActive: true, // Ẩn checkbox nhưng luôn true
  });

  useEffect(() => {
    // Lấy user hiện tại
    axiosInstance
      .get("/User", { params: { keyword: "", role: "", isActive: true } })
      .then((res) => {
        const found = res.data.find((u) => u.id == id);
        if (found) {
          setUser(found);
          setFormData({
            id: found.id,
            fullName: found.fullName || "",
            email: found.email || "",
            phoneNumber: found.phoneNumber || "",
            departmentId: found.departmentId || "",
            roles: found.roles || [], // Lấy roles từ user
            isActive: true, // Luôn true, không lấy từ backend
          });
        }
      });

    // Lấy danh sách phòng ban
    axiosInstance.get("/Departments").then((res) => setDepartments(res.data));

    // Giả sử roles cố định cho demo, hoặc bạn có thể fetch API roles nếu có
    setRoles(["Admin", "User"]);
  }, [id]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    const newValue = type === "checkbox" ? checked : value;
    setFormData((prev) => ({ ...prev, [name]: newValue }));
  };

  const handleRoleChange = (role) => {
    setFormData((prev) => ({
      ...prev,
      roles: prev.roles.includes(role)
        ? prev.roles.filter((r) => r !== role)
        : [...prev.roles, role],
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await axiosInstance.put("/User", formData);
      alert("Cập nhật thành công");
      navigate("/users");
    } catch (err) {
      console.log(err);
      alert("Cập nhật thất bại");
    }
  };

  if (!user) return <div>Đang tải...</div>;

  return (
    <div className="max-w-xl mx-auto p-4">
      <h2 className="text-xl font-bold mb-4">Chỉnh sửa người dùng</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          type="text"
          name="fullName"
          value={formData.fullName}
          onChange={handleChange}
          placeholder="Họ tên"
          className="w-full p-2 border rounded"
          required
        />

        <input
          type="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
          placeholder="Email"
          className="w-full p-2 border rounded"
        />

        <input
          type="tel"
          name="phoneNumber"
          value={formData.phoneNumber}
          onChange={handleChange}
          placeholder="Số điện thoại"
          className="w-full p-2 border rounded"
        />

        <select
          name="departmentId"
          value={formData.departmentId}
          onChange={handleChange}
          className="w-full p-2 border rounded"
        >
          <option value="">Chọn phòng ban</option>
          {departments.map((dep) => (
            <option key={dep.id} value={dep.id}>
              {dep.name}
            </option>
          ))}
        </select>

        {/* Roles checkbox */}
        <div>
          <label className="block font-medium mb-1">Vai trò:</label>
          {roles.map((role) => (
            <label key={role} className="inline-flex items-center mr-4">
              <input
                type="checkbox"
                checked={formData.roles.includes(role)}
                onChange={() => handleRoleChange(role)}
                className="mr-1"
              />
              {role}
            </label>
          ))}
        </div>

        {/* isActive ẩn, giữ luôn true */}
        <input type="hidden" name="isActive" value="true" />

        <button
          type="submit"
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Cập nhật
        </button>
      </form>
    </div>
  );
}

export default EditUserForm;
