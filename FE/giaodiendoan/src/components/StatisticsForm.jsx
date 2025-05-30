import React, { useState, useEffect } from "react";
import { API_URL } from "../config";

const StatisticsForm = () => {
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [keyword, setKeyword] = useState(""); // từ khóa khi nhấn nút tìm
  const [data, setData] = useState([]);

  // Gọi API lấy dữ liệu khi ngày thay đổi
  useEffect(() => {
    const fetchStatistics = async () => {
      if (!fromDate || !toDate) return;
      try {
        const res = await fetch(
          `${API_URL}/Statistics/all?from=${fromDate}&to=${toDate}`
        );
        const result = await res.json();
        setData(result);
      } catch (error) {
        console.error("Lỗi khi lấy dữ liệu:", error);
      }
    };

    fetchStatistics();
  }, [fromDate, toDate]);

  // Gọi API export excel
  const exportExcel = async () => {
    if (!fromDate || !toDate) return;
    try {
      const res = await fetch(
        `${API_URL}/Statistics/export-excel?from=${fromDate}&to=${toDate}`
      );
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "AttendanceStats.xlsx";
      a.click();
    } catch (error) {
      console.error("Lỗi khi xuất Excel:", error);
    }
  };

  // Lọc khi người dùng nhấn nút "Tìm kiếm"
  const handleSearch = () => {
    setKeyword(searchTerm.trim());
  };

  const filteredData = data.filter(
    (item) =>
      item.name?.toLowerCase().includes(keyword.toLowerCase()) ||
      item.id?.toString().includes(keyword)
  );

  return (
    <div className="statistics-container">
      {/* Bộ lọc ngày và xuất Excel */}
      <div className="date-filter">
        <label>
          Từ:{" "}
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
          />
        </label>{" "}
        <label>
          Đến:{" "}
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
          />
        </label>{" "}
        <button onClick={exportExcel} style={{ marginLeft: "10px" }}>
          Xuất Excel
        </button>
      </div>

      {/* Tìm kiếm */}
      <div className="search-section">
        <input
          type="text"
          placeholder="Tìm theo tên hoặc ID..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ flex: 1, padding: "5px" }}
        />
        <button onClick={handleSearch}>Tìm kiếm</button>
      </div>

      {/* Bảng kết quả */}
      <table className="stats-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Tên</th>
            <th>Tổng công</th>
            <th>Tăng ca</th>
            <th>Nghỉ phép</th>
          </tr>
        </thead>
        <tbody>
          {filteredData.length === 0 ? (
            <tr>
              <td
                colSpan="5"
                style={{ textAlign: "center", fontStyle: "italic" }}
              >
                {keyword
                  ? "Không tìm thấy nhân viên nào."
                  : "Không có dữ liệu."}
              </td>
            </tr>
          ) : (
            filteredData.map((item) => (
              <tr key={item.id}>
                <td>{item.id}</td>
                <td>{item.name}</td>
                <td>{item.totalWorkdays}</td>
                <td>{item.overtimeHours}</td>
                <td>{item.leaveDays}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};

export default StatisticsForm;
