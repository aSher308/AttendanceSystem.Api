import React, { useState, useEffect, useCallback } from "react";
import axiosInstance from "../utils/axiosInstance";
import { format, isAfter, parseISO } from "date-fns";
import { useNavigate } from "react-router-dom";

const XemLichLamForm = () => {
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [dateRange, setDateRange] = useState({
    fromDate: format(new Date(), "yyyy-MM-01"),
    toDate: format(new Date(), "yyyy-MM-dd"),
  });
  const navigate = useNavigate();

  const userId = localStorage.getItem("userId");
  const userIdNumber = userId ? parseInt(userId, 10) : null;

  const formatTime = (timeStr) => {
    if (!timeStr) return "N/A";
    return timeStr.slice(0, 5);
  };

  const validateSession = useCallback(() => {
    if (!userIdNumber || userIdNumber <= 0) {
      localStorage.removeItem("userId");
      localStorage.removeItem("token");
      setError("Phiên đăng nhập không hợp lệ, vui lòng đăng nhập lại");
      navigate("/login");
      return false;
    }
    return true;
  }, [userIdNumber, navigate]);

  const fetchWorkSchedules = useCallback(async () => {
    if (!validateSession()) return;

    // Validate date range
    if (isAfter(parseISO(dateRange.fromDate), parseISO(dateRange.toDate))) {
      setError("Ngày bắt đầu không thể sau ngày kết thúc");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const params = {
        fromDate: dateRange.fromDate,
        toDate: dateRange.toDate,
        userId: userIdNumber,
      };

      const response = await axiosInstance.get("/WorkSchedule", { params });

      if (response.data?.Success === false) {
        throw new Error(response.data.Message || "Lỗi khi tải dữ liệu");
      }

      setSchedules(response.data?.Data || response.data || []);
    } catch (err) {
      console.error("Error fetching work schedules:", err);
      const errorMessage =
        err.response?.data?.Message ||
        err.response?.data?.message ||
        err.response?.data?.error ||
        err.message ||
        "Lỗi khi tải dữ liệu lịch làm việc";

      setError(errorMessage);

      // Nếu lỗi 401 Unauthorized thì đăng xuất
      if (err.response?.status === 401) {
        localStorage.removeItem("userId");
        localStorage.removeItem("token");
        navigate("/login");
      }
    } finally {
      setLoading(false);
    }
  }, [dateRange, userIdNumber, navigate, validateSession]);

  useEffect(() => {
    fetchWorkSchedules();
  }, [fetchWorkSchedules]);

  const handleDateChange = (e) => {
    const { name, value } = e.target;
    setDateRange((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  return (
    <div className="work-schedule-container">
      <h2>Lịch làm việc cá nhân</h2>

      <div className="filter-section">
        <div className="date-filter">
          <label>Từ ngày:</label>
          <input
            type="date"
            name="fromDate"
            value={dateRange.fromDate}
            onChange={handleDateChange}
          />
        </div>
        <div className="date-filter">
          <label>Đến ngày:</label>
          <input
            type="date"
            name="toDate"
            value={dateRange.toDate}
            onChange={handleDateChange}
          />
        </div>
        <button onClick={fetchWorkSchedules} className="refresh-btn">
          Làm mới
        </button>
      </div>

      {error && <div className="error-message">{error.toString()}</div>}

      {loading ? (
        <div>Đang tải dữ liệu...</div>
      ) : (
        <div className="schedule-table-container">
          <table className="schedule-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>Ngày</th>
                <th>Ca làm</th>
                <th>Giờ bắt đầu</th>
                <th>Giờ kết thúc</th>
              </tr>
            </thead>
            <tbody>
              {schedules.length > 0 ? (
                schedules.map((schedule, index) => (
                  <tr key={schedule.id}>
                    <td>{index + 1}</td>
                    <td>
                      {schedule.workDate
                        ? format(new Date(schedule.workDate), "dd/MM/yyyy")
                        : ""}
                    </td>
                    <td>{schedule.shiftName}</td>
                    <td>{formatTime(schedule.startTime)}</td>
                    <td>{formatTime(schedule.endTime)}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="no-data">
                    Không có dữ liệu lịch làm việc
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default XemLichLamForm;
