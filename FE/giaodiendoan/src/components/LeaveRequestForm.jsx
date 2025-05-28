import React, { useState } from "react";
import axios from "axios";
import { API_URL } from "../config";

export default function LeaveRequestForm() {
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [reason, setReason] = useState("");
  const [message, setMessage] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const userId = localStorage.getItem("userId");
      await axios.post(`${API_URL}/leave-requests`, {
        userId,
        fromDate,
        toDate,
        reason,
      });
      setMessage("✅ Gửi đơn nghỉ phép thành công!");
      setFromDate("");
      setToDate("");
      setReason("");
    } catch (err) {
      setMessage("❌ Gửi đơn thất bại!");
    }
  };

  return (
    <div className="shift-form-container" style={{ maxWidth: 500 }}>
      <h2 style={{ textAlign: "center", marginBottom: 24 }}>Đơn xin nghỉ phép</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Từ ngày:</label>
          <input
            type="date"
            className="form-control"
            value={fromDate}
            onChange={e => setFromDate(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label>Đến ngày:</label>
          <input
            type="date"
            className="form-control"
            value={toDate}
            onChange={e => setToDate(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label>Lý do:</label>
          <textarea
            className="form-control"
            rows={3}
            value={reason}
            onChange={e => setReason(e.target.value)}
            placeholder="Nhập lý do xin nghỉ phép..."
            required
          />
        </div>
        <div className="form-actions">
          <button type="submit" className="btn btn-primary">
            Gửi đơn
          </button>
        </div>
        {message && (
          <div className="alert-info" style={{ marginTop: 16 }}>
            {message}
          </div>
        )}
      </form>
    </div>
  );
}