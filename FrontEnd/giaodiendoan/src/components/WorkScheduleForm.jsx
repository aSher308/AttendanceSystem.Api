import React, { useState, useEffect } from "react";
import axios from "axios";
import { API_URL } from "../config";
import "../styles/style.css";

const WorkScheduleForm = () => {
  const [formData, setFormData] = useState({
    userId: "",
    date: "",
    shift: "",
  });

  const [schedules, setSchedules] = useState([]);
  const [updateId, setUpdateId] = useState(null);

  //DS lịch
  const fetchSchedules = async () => {
    try {
      const res = await axios.get(`${API_URL}/WorkSchedule`);
      setSchedules(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy lịch làm:", err);
    }
  };
  useEffect(() => {
    fetchSchedules();
  }, []);

  // Thay đổi dữ liệu form
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  // Tạo mới hoặc cập nhật lịch làm
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (updateId) {
        // Cập nhật
        await axios.put(`${API_URL}/WorkSchedule`, {
          id: updateId,
          ...formData,
        });
        alert("Cập nhật lịch làm thành công!");
        setUpdateId(null);
      } else {
        // Tạo mới
        await axios.post(`${API_URL}/WorkSchedule`, formData);
        alert("Tạo lịch làm thành công!");
      }
      setFormData({ userId: "", date: "", shift: "" });
      fetchSchedules();
    } catch (err) {
      console.error("Lỗi:", err);
    }
  };

  // Chọn lịch làm để sửa
  const handleEdit = (schedule) => {
    setFormData({
      userId: schedule.userId,
      date: schedule.date,
      shift: schedule.shift,
    });
    setUpdateId(schedule.id);
  };

  // Xóa lịch làm
  const handleDelete = async (id) => {
    if (confirm("Bạn có chắc chắn muốn xóa lịch làm này không?")) {
      try {
        await axios.delete(`${API_URL}/WorkSchedule/${id}`);
        alert("Xóa thành công!");
        fetchSchedules();
      } catch (err) {
        console.error("Lỗi xóa:", err);
      }
    }
  };

  return (
    <div className="LichLam">
      <h2 className="TieuDe">{updateId ? "Cập nhật" : "Tạo"} lịch làm</h2>
      <form onSubmit={handleSubmit} className="space-y-3">
        <input
          type="number"
          name="userId"
          placeholder="User ID"
          value={formData.userId}
          onChange={handleChange}
          required
          className="w-full border p-2 rounded"
        />
        <input
          type="date"
          name="date"
          value={formData.date}
          onChange={handleChange}
          required
          className="w-full border p-2 rounded"
        />
        <input
          type="text"
          name="shift"
          placeholder="Ca làm (VD: Sáng/Chiều)"
          value={formData.shift}
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

      <h3 className="text-lg font-semibold mt-8 mb-2">Danh sách lịch làm</h3>
      <ul className="space-y-2">
        {schedules.map((s) => (
          <li
            key={s.id}
            className="border p-3 rounded flex justify-between items-center"
          >
            <div>
              <p>
                <strong>User ID:</strong> {s.userId}
              </p>
              <p>
                <strong>Ngày:</strong> {new Date(s.date).toLocaleDateString()}
              </p>
              <p>
                <strong>Ca:</strong> {s.shift}
              </p>
            </div>
            <div className="space-x-2">
              <button
                onClick={() => handleEdit(s)}
                className="bg-yellow-400 px-2 py-1 rounded"
              >
                Sửa
              </button>
              <button
                onClick={() => handleDelete(s.id)}
                className="bg-red-500 text-white px-2 py-1 rounded"
              >
                Xóa
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default WorkScheduleForm;
