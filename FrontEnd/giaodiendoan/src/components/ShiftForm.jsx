import React, { useEffect, useState } from "react";
import axios from "axios";
import { API_URL } from "../config";
import "../styles/style.css";

const ShiftForm = () => {
  const [shifts, setShifts] = useState([]);
  const [formData, setFormData] = useState({
    name: "",
    startTime: "",
    endTime: "",
    isActive: true, // thêm mặc định true
    description: "", // thêm trường mô tả
  });
  const [updateId, setUpdateId] = useState(null);

  // Lấy danh sách ca làm
  const fetchShifts = async () => {
    try {
      const res = await axios.get(`${API_URL}/Shift`);
      setShifts(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy danh sách ca làm:", err);
    }
  };

  useEffect(() => {
    fetchShifts();
  }, []);

  // Xử lý thay đổi dữ liệu form
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    if (type === "checkbox") {
      setFormData((prev) => ({ ...prev, [name]: checked }));
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  // Tạo hoặc cập nhật ca làm
  const handleSubmit = async (e) => {
    e.preventDefault();

    const dataToSend = {
      name: formData.name,
      startTime: formData.startTime + ":00",
      endTime: formData.endTime + ":00",
      isActive: formData.isActive,
      description: formData.description || null,
    };
    console.log("Gửi dữ liệu:", dataToSend);

    try {
      if (updateId) {
        await axios.put(`${API_URL}/Shift/Update`, {
          id: updateId,
          ...dataToSend,
        });
        alert("Cập nhật ca làm thành công!");
      } else {
        await axios.post(`${API_URL}/Shift/Create`, dataToSend);
        alert("Tạo ca làm thành công!");
      }
      setFormData({
        name: "",
        startTime: "",
        endTime: "",
        isActive: true,
        description: "",
      });
      setUpdateId(null);
      fetchShifts();
    } catch (err) {
      console.error("Lỗi khi lưu:", err);
    }
  };

  // Chọn ca làm để sửa
  const handleEdit = (shift) => {
    // Bỏ :00 nếu có để hiển thị đúng vào input type="time"
    const trimTime = (time) => (time?.length === 8 ? time.slice(0, 5) : time);

    setFormData({
      name: shift.name,
      startTime: trimTime(shift.startTime),
      endTime: trimTime(shift.endTime),
      isActive: shift.isActive,
      description: shift.description || "",
    });
    setUpdateId(shift.id);
  };

  // Xóa ca làm
  const handleDelete = async (id) => {
    if (confirm("Bạn có chắc muốn xóa ca làm này?")) {
      try {
        await axios.delete(`${API_URL}/Shift/${id}`);
        alert("Đã xóa ca làm!");
        fetchShifts();
      } catch (err) {
        console.error("Lỗi khi xóa:", err);
      }
    }
  };

  // Thay đổi trạng thái ca làm
  const toggleStatus = async (id, currentStatus) => {
    try {
      await axios.patch(
        `${API_URL}/Shift/${id}/status?isActive=${!currentStatus}`
      );
      alert("Cập nhật trạng thái thành công!");
      fetchShifts();
    } catch (err) {
      console.error("Lỗi khi cập nhật trạng thái:", err);
    }
  };

  return (
    <div className="shift-form-container">
      <h2 className="text-xl font-bold mb-4">
        {updateId ? "Cập nhật" : "Tạo"} Ca Làm
      </h2>
      <form onSubmit={handleSubmit} className="space-y-3">
        <input
          type="text"
          name="name"
          placeholder="Tên ca làm"
          value={formData.name}
          onChange={handleChange}
          required
          className="w-full border p-2 rounded"
        />
        <input
          type="time"
          name="startTime"
          value={formData.startTime}
          onChange={handleChange}
          required
          className="w-full border p-2 rounded"
        />
        <input
          type="time"
          name="endTime"
          value={formData.endTime}
          onChange={handleChange}
          required
          className="w-full border p-2 rounded"
        />

        <button
          type="submit"
          className="bg-blue-500 text-white px-4 py-2 rounded"
        >
          {updateId ? "Cập nhật" : "Tạo"}
        </button>
      </form>

      <h3 className="text-lg font-semibold mt-8 mb-2">Danh sách ca làm</h3>
      <ul className="shift-list">
        {shifts.map((shift) => (
          <li key={shift.id} className="shift-item">
            <div>
              <p>
                <strong>{shift.name}</strong>
              </p>
              <p>
                🕒 {shift.startTime} - {shift.endTime}
              </p>
              <p>
                Trạng thái:{" "}
                {shift.isActive ? (
                  <span className="text-green">✅ Active</span>
                ) : (
                  <span className="text-red">❌ Inactive</span>
                )}
              </p>
            </div>
            <div className="shift-actions">
              <button
                onClick={() => handleEdit(shift)}
                className="btn btn-edit"
              >
                Sửa
              </button>
              <button
                onClick={() => handleDelete(shift.id)}
                className="btn btn-delete"
              >
                Xóa
              </button>
              <button
                onClick={() => toggleStatus(shift.id, shift.isActive)}
                className="btn btn-toggle"
              >
                {shift.isActive ? "Tắt" : "Bật"}
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ShiftForm;
