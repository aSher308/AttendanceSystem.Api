import React, { useState, useEffect, useCallback } from "react";
import axiosInstance from "../utils/axiosInstance";
import "../styles/style.css";

const WorkScheduleForm = () => {
  const [formData, setFormData] = useState({
    userId: "",
    shiftId: "",
    workDate: "",
    note: "",
    status: "Active",
  });

  const [schedules, setSchedules] = useState([]);
  const [updateId, setUpdateId] = useState(null);
  const [employees, setEmployees] = useState([]);
  const [shifts, setShifts] = useState([]);
  const [filterMonth, setFilterMonth] = useState(new Date().getMonth() + 1);
  const [filterYear, setFilterYear] = useState(new Date().getFullYear());
  const [loading, setLoading] = useState(false);

  // Lấy danh sách nhân viên
  const fetchEmployees = async () => {
    try {
      const res = await axiosInstance.get(`/User`);
      setEmployees(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy danh sách nhân viên:", err);
    }
  };

  // Lấy danh sách ca làm
  const fetchShifts = async () => {
    try {
      const res = await axiosInstance.get(`/Shift`);
      setShifts(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy danh sách ca làm:", err);
    }
  };

  // Lấy lịch làm theo tháng năm lọc
  const fetchSchedules = useCallback(async () => {
    setLoading(true);
    try {
      const firstDay = new Date(filterYear, filterMonth - 1, 1);
      const lastDay = new Date(filterYear, filterMonth, 0);

      const res = await axiosInstance.get(`/WorkSchedule`, {
        params: {
          fromDate: firstDay.toISOString(),
          toDate: lastDay.toISOString(),
        },
      });
      setSchedules(res.data);
    } catch (err) {
      console.error("Lỗi khi lấy lịch làm:", err);
    } finally {
      setLoading(false);
    }
  }, [filterMonth, filterYear]);

  useEffect(() => {
    fetchEmployees();
    fetchShifts();
  }, []);

  useEffect(() => {
    fetchSchedules();
  }, [fetchSchedules]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const resetForm = () => {
    setFormData({
      userId: "",
      shiftId: "",
      workDate: "",
      note: "",
      status: "Active",
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        ...formData,
        workDate: new Date(formData.workDate).toISOString(),
      };

      if (updateId) {
        // Cập nhật lịch làm
        await axiosInstance.put(`/WorkSchedule`, {
          ...payload,
          id: updateId,
        });
        alert("Cập nhật lịch làm thành công!");
        setUpdateId(null);
      } else {
        // Tạo mới lịch làm
        await axiosInstance.post(`/WorkSchedule`, payload);
        alert("Tạo lịch làm thành công!");
      }

      resetForm();
      fetchSchedules();
    } catch (err) {
      console.error("Lỗi:", err.response?.data || err.message);
      alert("Có lỗi xảy ra: " + (err.response?.data?.message || err.message));
    }
  };

  const handleEdit = (schedule) => {
    setFormData({
      userId: schedule.userId,
      shiftId: schedule.shiftId,
      workDate: schedule.workDate.split("T")[0],
      note: schedule.note || "",
      status: schedule.status,
    });
    setUpdateId(schedule.id);
  };

  const handleDelete = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa lịch làm này không?")) {
      try {
        await axiosInstance.delete(`/WorkSchedule/${id}`);
        alert("Xóa lịch làm thành công!");
        fetchSchedules();
      } catch (err) {
        console.error("Lỗi xóa:", err);
        alert(
          "Có lỗi xảy ra khi xóa: " +
            (err.response?.data?.message || err.message)
        );
      }
    }
  };

  // Tạo danh sách năm (5 năm trước đến 5 năm sau)
  const generateYearOptions = () => {
    const currentYear = new Date().getFullYear();
    return Array.from({ length: 11 }, (_, i) => currentYear - 5 + i);
  };

  return (
    <div className="work-schedule-container">
      <h1 className="header">Quản lý lịch làm việc</h1>

      {/* Form tạo/cập nhật */}
      <div className="form-section">
        <h2>{updateId ? "Cập nhật" : "Tạo mới"} lịch làm việc</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Nhân viên:</label>
            <select
              name="userId"
              value={formData.userId}
              onChange={handleChange}
              required
              className="form-control"
            >
              <option value="">-- Chọn nhân viên --</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}{" "}
                  {emp.departmentName ? `(${emp.departmentName})` : ""}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Ca làm việc:</label>
            <select
              name="shiftId"
              value={formData.shiftId}
              onChange={handleChange}
              required
              className="form-control"
            >
              <option value="">-- Chọn ca làm --</option>
              {shifts.map((shift) => (
                <option key={shift.id} value={shift.id}>
                  {shift.name} ({shift.startTime} - {shift.endTime})
                  {!shift.isActive && " (Đã tắt)"}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Ngày làm việc:</label>
            <input
              type="date"
              name="workDate"
              value={formData.workDate}
              onChange={handleChange}
              required
              className="form-control"
            />
          </div>

          <div className="form-group">
            <label>Ghi chú:</label>
            <input
              type="text"
              name="note"
              value={formData.note}
              onChange={handleChange}
              className="form-control"
              placeholder="Nhập ghi chú (nếu có)"
            />
          </div>

          <div className="form-group">
            <label>Trạng thái:</label>
            <select
              name="status"
              value={formData.status}
              onChange={handleChange}
              className="form-control"
              required
            >
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-primary">
              {updateId ? "Cập nhật" : "Tạo mới"}
            </button>
            {updateId && (
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => {
                  setUpdateId(null);
                  resetForm();
                }}
              >
                Hủy
              </button>
            )}
          </div>
        </form>
      </div>

      {/* Bộ lọc tháng/năm */}
      <div className="filter-section">
        <div className="filter-group">
          <label>Tháng:</label>
          <select
            value={filterMonth}
            onChange={(e) => setFilterMonth(parseInt(e.target.value))}
            className="filter-input"
          >
            {Array.from({ length: 12 }, (_, i) => i + 1).map((month) => (
              <option key={month} value={month}>
                Tháng {month}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label>Năm:</label>
          <select
            value={filterYear}
            onChange={(e) => setFilterYear(parseInt(e.target.value))}
            className="filter-input"
          >
            {generateYearOptions().map((year) => (
              <option key={year} value={year}>
                Năm {year}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Danh sách lịch làm việc */}
      <div className="schedule-list-section">
        <h2>
          Danh sách lịch làm việc tháng {filterMonth}/{filterYear}
        </h2>

        {loading ? (
          <div className="loading">Đang tải dữ liệu...</div>
        ) : schedules.length === 0 ? (
          <div className="no-data">
            Không tìm thấy lịch làm việc nào trong tháng {filterMonth}/
            {filterYear}
          </div>
        ) : (
          <div className="schedule-table-container">
            <table className="schedule-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Nhân viên</th>
                  <th>Ngày làm</th>
                  <th>Ca làm</th>
                  <th>Thời gian</th>
                  <th>Trạng thái</th>
                  <th>Ghi chú</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {schedules.map((schedule, index) => (
                  <tr key={schedule.id}>
                    <td>{index + 1}</td>
                    <td>{schedule.fullName}</td>
                    <td>
                      {new Date(schedule.workDate).toLocaleDateString("vi-VN")}
                    </td>
                    <td>{schedule.shiftName}</td>
                    <td>
                      {schedule.startTime} - {schedule.endTime}
                    </td>
                    <td>
                      <span
                        className={`status-badge ${
                          schedule.status === "Active" ? "active" : "inactive"
                        }`}
                      >
                        {schedule.status}
                      </span>
                    </td>
                    <td>{schedule.note || "-"}</td>
                    <td>
                      <button
                        onClick={() => handleEdit(schedule)}
                        className="btn btn-edit"
                      >
                        Sửa
                      </button>
                      <button
                        onClick={() => handleDelete(schedule.id)}
                        className="btn btn-delete"
                      >
                        Xóa
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default WorkScheduleForm;
