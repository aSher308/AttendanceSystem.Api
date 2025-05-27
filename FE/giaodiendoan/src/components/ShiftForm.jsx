import React, { useEffect, useState } from "react";
import axiosInstance from "../utils/axiosInstance"; // import axiosInstance đã config sẵn token
import { API_URL } from "../config";
import "../styles/style.css";

const ShiftForm = () => {
  const [shifts, setShifts] = useState([]);
  const [formData, setFormData] = useState({
    name: "",
    startHour: "08",
    startMinute: "00",
    startSecond: "00",
    endHour: "17",
    endMinute: "00",
    endSecond: "00",
    isActive: true,
    description: "",
  });
  const [updateId, setUpdateId] = useState(null);

  const fetchShifts = async () => {
    try {
      const res = await axiosInstance.get(`${API_URL}/Shift`);
      setShifts(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy danh sách ca làm:", err);
    }
  };

  useEffect(() => {
    fetchShifts();
  }, []);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    if (name === "isActive") return;

    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const getTimeString = (h, m, s) => `${h}:${m}:${s}`;

  const handleSubmit = async (e) => {
    e.preventDefault();

    const startTime = getTimeString(
      formData.startHour,
      formData.startMinute,
      formData.startSecond
    );
    const endTime = getTimeString(
      formData.endHour,
      formData.endMinute,
      formData.endSecond
    );

    const dataToSend = {
      name: formData.name,
      startTime,
      endTime,
      isActive: true,
      description: formData.description || null,
    };

    try {
      if (updateId) {
        await axiosInstance.put(`${API_URL}/Shift`, {
          id: updateId,
          ...dataToSend,
        });
        alert("Cập nhật ca làm thành công!");
      } else {
        await axiosInstance.post(`${API_URL}/Shift`, dataToSend);
        alert("Tạo ca làm thành công!");
      }

      setFormData({
        name: "",
        startHour: "08",
        startMinute: "00",
        startSecond: "00",
        endHour: "17",
        endMinute: "00",
        endSecond: "00",
        isActive: true,
        description: "",
      });
      setUpdateId(null);
      fetchShifts();
    } catch (err) {
      console.error("Lỗi khi lưu:", err);
      console.error("Chi tiết lỗi:", err.response?.data);
    }
  };

  const handleEdit = (shift) => {
    const [sHour, sMin, sSec] = shift.startTime.split(":");
    const [eHour, eMin, eSec] = shift.endTime.split(":");
    setFormData({
      name: shift.name,
      startHour: sHour,
      startMinute: sMin,
      startSecond: sSec,
      endHour: eHour,
      endMinute: eMin,
      endSecond: eSec,
      isActive: true,
      description: shift.description || "",
    });
    setUpdateId(shift.id);
  };

  const handleDelete = async (id) => {
    if (confirm("Bạn có chắc muốn xóa ca làm này?")) {
      try {
        await axiosInstance.delete(`${API_URL}/Shift/${id}`);
        alert("Đã xóa ca làm!");
        fetchShifts();
      } catch (err) {
        console.error("Lỗi khi xóa:", err);
      }
    }
  };

  const toggleStatus = async (id, currentStatus) => {
    try {
      await axiosInstance.patch(
        `${API_URL}/Shift/${id}/status?isActive=${!currentStatus}`
      );
      alert("Cập nhật trạng thái thành công!");
      fetchShifts();
    } catch (err) {
      console.error("Lỗi khi cập nhật trạng thái:", err);
    }
  };

  const renderTimeOptions = (count) =>
    Array.from({ length: count }, (_, i) => {
      const val = String(i).padStart(2, "0");
      return (
        <option key={val} value={val}>
          {val}
        </option>
      );
    });

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
        <label>Giờ bắt đầu:</label>
        <div className="flex items-center space-x-1">
          <select
            name="startHour"
            value={formData.startHour}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(24)}
          </select>
          <span>:</span>
          <select
            name="startMinute"
            value={formData.startMinute}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(60)}
          </select>
          <span>:</span>
          <select
            name="startSecond"
            value={formData.startSecond}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(60)}
          </select>
        </div>
        <label>Giờ kết thúc:</label>
        <div className="flex items-center space-x-1">
          <select
            name="endHour"
            value={formData.endHour}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(24)}
          </select>
          <span>:</span>
          <select
            name="endMinute"
            value={formData.endMinute}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(60)}
          </select>
          <span>:</span>
          <select
            name="endSecond"
            value={formData.endSecond}
            onChange={handleChange}
            className="border p-1 rounded"
          >
            {renderTimeOptions(60)}
          </select>
        </div>{" "}
        <br />
        <input
          type="text"
          name="description"
          placeholder="Mô tả (tùy chọn)"
          value={formData.description}
          onChange={handleChange}
          className="w-full border p-2 rounded"
        />
        <input type="hidden" name="isActive" value={true} />
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
