import React, { useEffect, useState } from "react";
import axiosInstance from "../utils/axiosInstance";
import { hasRole } from "../utils/roleUtils";

function LichNghiPhepCaNhanForm() {
  const [requests, setRequests] = useState([]);
  const [form, setForm] = useState({
    fromDate: "",
    toDate: "",
    reason: "",
    leaveType: 0, // Default: PaidLeave
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const userId = parseInt(localStorage.getItem("userId"));

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const res = await axiosInstance.get("/LeaveRequest");
      setRequests(res.data);
      setError(null);
    } catch (err) {
      setError("Không thể tải dữ liệu đơn nghỉ phép");
      console.error("Fetch leave requests error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);

      const requestData = {
        userId: userId,
        fromDate: new Date(form.fromDate).toISOString(),
        toDate: new Date(form.toDate).toISOString(),
        reason: form.reason,
        leaveType: parseInt(form.leaveType),
        createdBy: userId,
      };

      await axiosInstance.post("/LeaveRequest", requestData);
      setForm({ fromDate: "", toDate: "", reason: "", leaveType: 0 });
      await fetchData();
    } catch (err) {
      setError("Gửi đơn nghỉ phép thất bại");
      console.error("Create leave request error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdate = async (id) => {
    if (!hasRole("Employee")) {
      alert("Bạn không có quyền chỉnh sửa đơn nghỉ phép");
      return;
    }

    const updated = prompt("Cập nhật lý do nghỉ:");
    if (updated) {
      try {
        setIsLoading(true);
        await axiosInstance.put("/LeaveRequest", { id, reason: updated });
        await fetchData();
      } catch (err) {
        setError("Cập nhật đơn nghỉ phép thất bại");
        console.error("Update leave request error:", err);
      } finally {
        setIsLoading(false);
      }
    }
  };

  const handleDelete = async (id) => {
    if (!hasRole("Employee")) {
      alert("Bạn không có quyền xóa đơn nghỉ phép");
      return;
    }

    if (!window.confirm("Bạn có chắc chắn muốn xóa đơn nghỉ phép này?")) {
      return;
    }

    try {
      setIsLoading(true);
      await axiosInstance.delete(`/LeaveRequest/${id}`);
      await fetchData();
    } catch (err) {
      setError("Xóa đơn nghỉ phép thất bại");
      console.error("Delete leave request error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="p-4">
      <h2 className="text-xl font-bold mb-4">Đơn nghỉ phép của tôi</h2>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleCreate} className="space-y-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">Từ ngày</label>
            <input
              type="date"
              required
              value={form.fromDate}
              onChange={(e) => setForm({ ...form, fromDate: e.target.value })}
              className="border p-2 rounded w-full"
              min={new Date().toISOString().split("T")[0]}
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Đến ngày</label>
            <input
              type="date"
              required
              value={form.toDate}
              onChange={(e) => setForm({ ...form, toDate: e.target.value })}
              className="border p-2 rounded w-full"
              min={form.fromDate || new Date().toISOString().split("T")[0]}
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">
            Loại nghỉ phép
          </label>
          <select
            value={form.leaveType}
            onChange={(e) => setForm({ ...form, leaveType: e.target.value })}
            className="border p-2 rounded w-full"
            required
          >
            <option value={0}>Nghỉ phép có lương</option>
            <option value={1}>Nghỉ bệnh</option>
            <option value={2}>Nghỉ không lương</option>
            <option value={3}>Làm việc từ xa</option>
            <option value={4}>Khác</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Lý do nghỉ</label>
          <textarea
            placeholder="Nhập lý do nghỉ phép"
            required
            value={form.reason}
            onChange={(e) => setForm({ ...form, reason: e.target.value })}
            className="border p-2 rounded w-full"
            rows={3}
          />
        </div>

        <button
          type="submit"
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded"
          disabled={isLoading}
        >
          {isLoading ? "Đang xử lý..." : "Tạo đơn"}
        </button>
      </form>

      {isLoading && requests.length === 0 ? (
        <div className="text-center py-4">Đang tải dữ liệu...</div>
      ) : requests.length === 0 ? (
        <div className="text-center py-4">Không có đơn nghỉ phép nào</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-gray-100">
                <th className="border p-2">Thời gian</th>
                <th className="border p-2">Lý do</th>
                <th className="border p-2">Trạng thái</th>
                <th className="border p-2">Hành động</th>
              </tr>
            </thead>
            <tbody>
              {requests.map((req) => (
                <tr key={req.id} className="hover:bg-gray-50">
                  <td className="border p-2">
                    {new Date(req.fromDate).toLocaleDateString()} -{" "}
                    {new Date(req.toDate).toLocaleDateString()}
                  </td>
                  <td className="border p-2">{req.reason}</td>
                  <td className="border p-2">
                    <span
                      className={`px-2 py-1 rounded-full text-xs ${
                        req.status === "Approved"
                          ? "bg-green-100 text-green-800"
                          : req.status === "Rejected"
                          ? "bg-red-100 text-red-800"
                          : "bg-yellow-100 text-yellow-800"
                      }`}
                    >
                      {req.status}
                    </span>
                  </td>
                  <td className="border p-2 space-x-2">
                    {req.status === "Pending" && (
                      <>
                        <button
                          onClick={() => handleUpdate(req.id)}
                          className="text-yellow-600 hover:text-yellow-800"
                          disabled={isLoading}
                        >
                          Sửa
                        </button>
                        <button
                          onClick={() => handleDelete(req.id)}
                          className="text-red-600 hover:text-red-800"
                          disabled={isLoading}
                        >
                          Xoá
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default LichNghiPhepCaNhanForm;
