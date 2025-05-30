// LeaveRequestAdminPage.jsx - Trang dành cho Admin
import React, { useEffect, useState } from "react";
import axiosInstance from "../utils/axiosInstance";
import { hasRole } from "../utils/roleUtils";

function QuanLyDonNghiPhepForm() {
  const [requests, setRequests] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const res = await axiosInstance.get("/LeaveRequest");
      setRequests(res.data);
      setError(null);
    } catch (err) {
      setError("Không thể tải danh sách đơn nghỉ phép");
      console.error("Fetch leave requests error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleApprove = async (id, isApproved) => {
    if (!hasRole("Admin")) {
      alert("Bạn không có quyền duyệt đơn nghỉ phép");
      return;
    }

    try {
      setIsLoading(true);

      const userId = parseInt(localStorage.getItem("userId"), 10); // lấy userId người duyệt

      // map trạng thái sang enum số tương ứng backend
      const statusEnum = isApproved ? 1 : 2; // Approved = 1, Rejected = 2

      await axiosInstance.put("/LeaveRequest/approve", {
        id: id,
        status: statusEnum,
        reviewerComment: "", // hoặc bạn có thể thêm UI để nhập comment
        approvedBy: userId,
      });

      await fetchData();
      setError(null);
    } catch (err) {
      setError(isApproved ? "Duyệt đơn thất bại" : "Từ chối đơn thất bại");
      console.error("Approve/Reject error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="p-4">
      <h2 className="text-xl font-bold mb-4">Quản lý đơn nghỉ phép</h2>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {isLoading && requests.length === 0 ? (
        <div className="text-center py-4">Đang tải dữ liệu...</div>
      ) : requests.length === 0 ? (
        <div className="text-center py-4">Không có đơn nghỉ phép nào</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-gray-100">
                <th className="border p-2">Nhân viên</th>
                <th className="border p-2">Thời gian</th>
                <th className="border p-2">Lý do</th>
                <th className="border p-2">Trạng thái</th>
                <th className="border p-2">Duyệt</th>
              </tr>
            </thead>
            <tbody>
              {requests.map((req) => (
                <tr key={req.id} className="hover:bg-gray-50">
                  <td className="border p-2">{req.employeeName}</td>
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
                          onClick={() => handleApprove(req.id, true)}
                          className="text-green-600 hover:text-green-800"
                          disabled={isLoading}
                        >
                          Duyệt
                        </button>
                        <button
                          onClick={() => handleApprove(req.id, false)}
                          className="text-red-600 hover:text-red-800"
                          disabled={isLoading}
                        >
                          Từ chối
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

export default QuanLyDonNghiPhepForm;
