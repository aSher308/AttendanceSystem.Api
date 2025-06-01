import React, { useState, useEffect, useRef } from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import axiosInstance from "../utils/axiosInstance";

// Cấu hình icon cho leaflet
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl:
    "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

const attendanceTypeMap = {
  GPS: "GPS",
  QR: "QR",
  FaceRecognition: "FaceRecognition",
  Manual: "Manual",
  Remote: "GPS",
};

const TimekeepingForm = () => {
  // State
  const [userId, setUserId] = useState(null);
  const [deviceInfo, setDeviceInfo] = useState("");
  const [latitude, setLatitude] = useState(null);
  const [longitude, setLongitude] = useState(null);
  const [photoUrl, setPhotoUrl] = useState("");
  const [message, setMessage] = useState("");
  const [attendanceType, setAttendanceType] = useState("GPS");
  const [showMap, setShowMap] = useState(false);
  const [showCamera, setShowCamera] = useState(false);
  const [currentLocationName, setCurrentLocationName] = useState("");
  const [isValidLocation, setIsValidLocation] = useState(false);
  const [lastCheckIn, setLastCheckIn] = useState(null);
  const [canCheckOut, setCanCheckOut] = useState(false);
  const [workSchedules, setWorkSchedules] = useState([]);
  const [selectedScheduleId, setSelectedScheduleId] = useState(null);
  const [attendanceHistory, setAttendanceHistory] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [checkInStatus, setCheckInStatus] = useState(null);

  const videoRef = useRef(null);
  const canvasRef = useRef(null);

  // Load userId và device info
  useEffect(() => {
    const storedUserId = localStorage.getItem("userId");
    if (storedUserId) {
      const parsedUserId = parseInt(storedUserId, 10);
      setUserId(parsedUserId);
      setDeviceInfo(navigator.userAgent);
      fetchTodaySchedules(parsedUserId);
      checkTodayAttendance(parsedUserId);
      fetchAttendanceHistory(parsedUserId);
    } else {
      setMessage(
        "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại."
      );
    }
  }, []);

  // Reset ảnh & camera khi đổi hình thức chấm công
  useEffect(() => {
    setPhotoUrl("");
    if (showCamera) stopCamera();
  }, [attendanceType]);

  // Kiểm tra vị trí khi có thay đổi latitude/longitude
  useEffect(() => {
    if (latitude && longitude) {
      checkLocationValidity(latitude, longitude);
    }
  }, [latitude, longitude]);

  // Hàm kiểm tra vị trí hợp lệ
  const checkLocationValidity = async (lat, lng) => {
    try {
      const response = await axiosInstance.post("/Location/check", {
        latitude: lat,
        longitude: lng,
      });

      setIsValidLocation(response.data.isValid);
      setCurrentLocationName(response.data.locationName || "");

      if (response.data.isValid) {
        setMessage(`Bạn đang ở trong khu vực: ${response.data.locationName}`);
      } else {
        setMessage("Vị trí hiện tại không hợp lệ để chấm công");
      }
    } catch (error) {
      console.error("Lỗi kiểm tra vị trí:", error);
      setIsValidLocation(false);
      setMessage("Không thể kiểm tra vị trí. Vui lòng thử lại.");
    }
  };

  // Lấy lịch làm việc hôm nay
  const fetchTodaySchedules = async (userId) => {
    try {
      const today = new Date();
      const fromDate = new Date(today.setHours(0, 0, 0, 0)).toISOString();
      const toDate = new Date(today.setHours(23, 59, 59, 999)).toISOString();

      const response = await axiosInstance.get("/WorkSchedule", {
        params: { userId, fromDate, toDate },
      });

      setWorkSchedules(response.data);

      if (response.data.length > 0) {
        setSelectedScheduleId(response.data[0].id);
      }
    } catch (error) {
      console.error("Lỗi lấy lịch làm việc:", error);
      setMessage("Không thể lấy thông tin ca làm việc.");
    }
  };

  // Lấy lịch sử chấm công
  const fetchAttendanceHistory = async (userId) => {
    try {
      const today = new Date();
      const fromDate = new Date(
        today.setDate(today.getDate() - 7)
      ).toISOString(); // Lấy dữ liệu 7 ngày
      const toDate = new Date().toISOString();

      const response = await axiosInstance.get("/attendance", {
        params: { userId, fromDate, toDate },
      });

      setAttendanceHistory(response.data);
    } catch (error) {
      console.error("Lỗi lấy lịch sử chấm công:", error);
    }
  };

  // Kiểm tra trạng thái chấm công hôm nay
  const checkTodayAttendance = async (userId) => {
    try {
      const today = new Date();
      const fromDate = new Date(today.setHours(0, 0, 0, 0)).toISOString();
      const toDate = new Date(today.setHours(23, 59, 59, 999)).toISOString();

      const response = await axiosInstance.get("/attendance", {
        params: { userId, fromDate, toDate },
      });

      if (response.data && response.data.length > 0) {
        const latest = response.data[0];
        setLastCheckIn(latest);
        setCanCheckOut(latest.checkIn && !latest.checkOut);
      }
    } catch (error) {
      console.error("Lỗi kiểm tra chấm công:", error);
    }
  };

  // Lấy vị trí GPS
  const getLocation = () => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setLatitude(pos.coords.latitude);
          setLongitude(pos.coords.longitude);
          setShowMap(true);
        },
        (err) => {
          console.error("Lỗi lấy vị trí:", err.message);
          setMessage(`Không thể lấy vị trí: ${err.message}`);
        }
      );
    } else {
      setMessage("Trình duyệt không hỗ trợ lấy vị trí.");
    }
  };

  // Bật camera
  const startCamera = async () => {
    try {
      setShowCamera(true);
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "user" },
      });
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
    } catch (err) {
      console.error("Lỗi truy cập camera:", err);
      setMessage(`Không thể truy cập camera: ${err.message}`);
    }
  };

  // Tắt camera
  const stopCamera = () => {
    if (videoRef.current?.srcObject) {
      videoRef.current.srcObject.getTracks().forEach((track) => track.stop());
    }
    setShowCamera(false);
  };

  // Chụp ảnh từ webcam
  const capturePhoto = () => {
    if (videoRef.current && canvasRef.current) {
      const context = canvasRef.current.getContext("2d");
      canvasRef.current.width = videoRef.current.videoWidth;
      canvasRef.current.height = videoRef.current.videoHeight;
      context.drawImage(videoRef.current, 0, 0);
      const imgData = canvasRef.current.toDataURL("image/png");
      setPhotoUrl(imgData);
      setMessage("Đã chụp ảnh thành công!");
      stopCamera();
    }
  };

  // Handle check-in
  const handleCheckIn = async () => {
    console.log(deviceInfo);
    setIsLoading(true);
    try {
      if (!selectedScheduleId) throw new Error("Vui lòng chọn ca làm việc.");

      if (attendanceType !== "Remote" && (!latitude || !longitude)) {
        throw new Error("Vui lòng lấy vị trí trước khi check-in.");
      }

      if (attendanceType === "Remote" && !photoUrl) {
        throw new Error("Vui lòng chụp ảnh xác thực khi làm từ xa.");
      }

      const schedule = workSchedules.find((w) => w.id === selectedScheduleId);
      if (!schedule) throw new Error("Ca làm việc không hợp lệ.");

      // Kiểm tra vị trí nếu không phải Remote
      if (attendanceType !== "Remote" && !isValidLocation) {
        throw new Error("Vị trí hiện tại không hợp lệ để chấm công");
      }

      const status = calculateCheckInStatus(schedule.startTime);
      setCheckInStatus(status);

      const request = {
        userId,
        locationName:
          currentLocationName ||
          (attendanceType === "Remote" ? "Làm từ xa" : "Văn phòng"),
        deviceInfo: navigator.userAgent || "Unknown Device",
        latitude: attendanceType === "Remote" ? null : latitude,
        longitude: attendanceType === "Remote" ? null : longitude,
        photoUrl:
          attendanceType === "FaceRecognition" || attendanceType === "Remote"
            ? photoUrl
            : null,
        attendanceType: attendanceTypeMap[attendanceType] || "GPS",
        workScheduleId: selectedScheduleId,
      };

      const response = await axiosInstance.post(
        "/attendance/check-in",
        request
      );
      setMessage("Check-in thành công!");
      setCanCheckOut(true);
      checkTodayAttendance(userId);
      fetchAttendanceHistory(userId);
    } catch (error) {
      console.error("Lỗi check-in:", error);
      setMessage(
        error.response?.data?.message || error.message || "Lỗi khi check-in"
      );
    } finally {
      setIsLoading(false);
    }
  };

  // Handle check-out
  const handleCheckOut = async () => {
    setIsLoading(true);
    try {
      if (!lastCheckIn) {
        throw new Error("Bạn chưa check-in hôm nay. Không thể check-out.");
      }

      const request = {
        userId,
        deviceInfo: navigator.userAgent || "Unknown Device",
        latitude: attendanceType === "Remote" ? null : latitude,
        longitude: attendanceType === "Remote" ? null : longitude,
        photoUrl: attendanceType === "FaceRecognition" ? photoUrl : null,
        workScheduleId: selectedScheduleId,
      };

      const response = await axiosInstance.post(
        "/attendance/check-out",
        request
      );
      setMessage("Check-out thành công!");
      setCanCheckOut(false);
      checkTodayAttendance(userId);
      fetchAttendanceHistory(userId);
    } catch (error) {
      console.error("Lỗi check-out:", error);
      setMessage(
        error.response?.data?.message || error.message || "Lỗi khi check-out"
      );
    } finally {
      setIsLoading(false);
    }
  };

  // Tính trạng thái check-in (sớm/đúng giờ/muộn)
  const calculateCheckInStatus = (scheduleStartTime) => {
    try {
      if (!scheduleStartTime) return null;

      const now = new Date();
      let [h, m, s] = scheduleStartTime.split(":");
      const scheduleStart = new Date(now);
      scheduleStart.setHours(
        parseInt(h, 10),
        parseInt(m, 10),
        parseInt(s || "0"),
        0
      );

      const diffMinutes = (now - scheduleStart) / (1000 * 60);

      if (diffMinutes < -15) return "Early"; // Sớm hơn 15 phút
      else if (diffMinutes > 15) return "Late"; // Muộn hơn 15 phút
      else return "On Time";
    } catch {
      return null;
    }
  };

  return (
    <div className="timekeeping-container">
      <h2>Chấm công</h2>

      {/* Chọn ca làm việc */}
      <div className="mb-3">
        <label>Ca làm việc:</label>
        <select
          className="form-control"
          value={selectedScheduleId || ""}
          onChange={(e) => setSelectedScheduleId(Number(e.target.value))}
          disabled={isLoading}
        >
          {workSchedules.length === 0 && (
            <option value="">Không có ca làm việc hôm nay</option>
          )}
          {workSchedules.map((schedule) => (
            <option key={schedule.id} value={schedule.id}>
              {schedule.shiftName} ({schedule.startTime} - {schedule.endTime})
            </option>
          ))}
        </select>
      </div>

      {/* Chọn hình thức chấm công */}
      <div className="mb-3">
        <label>Hình thức chấm công:</label>
        <select
          className="form-control"
          value={attendanceType}
          onChange={(e) => setAttendanceType(e.target.value)}
          disabled={isLoading}
        >
          <option value="GPS">Thông thường (GPS)</option>
          <option value="QR">Quét QR</option>
          <option value="FaceRecognition">Nhận diện khuôn mặt</option>
          <option value="Remote">Làm từ xa</option>
        </select>
      </div>

      {/* Nút lấy vị trí (ẩn nếu Remote) */}
      {attendanceType !== "Remote" && (
        <button
          className="btn btn-primary mb-3"
          onClick={getLocation}
          disabled={isLoading}
        >
          Lấy vị trí hiện tại
        </button>
      )}

      {/* Hiển thị thông tin vị trí */}
      {latitude && longitude && (
        <div
          className={`alert ${
            isValidLocation ? "alert-success" : "alert-warning"
          } mb-3`}
        >
          Vị trí hiện tại: {latitude.toFixed(6)}, {longitude.toFixed(6)}
          <br />
          {currentLocationName && `Khu vực: ${currentLocationName}`}
          {!isValidLocation && " - Không hợp lệ để chấm công"}
        </div>
      )}

      {/* Hiển thị bản đồ */}
      {showMap && latitude && longitude && (
        <div style={{ height: 300, marginBottom: 20 }}>
          <MapContainer
            center={[latitude, longitude]}
            zoom={16}
            style={{ height: "100%" }}
          >
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
            <Marker position={[latitude, longitude]}>
              <Popup>Vị trí hiện tại của bạn</Popup>
            </Marker>
          </MapContainer>
        </div>
      )}

      {/* Camera cho Remote hoặc FaceRecognition */}
      {(attendanceType === "Remote" || attendanceType === "FaceRecognition") &&
        !photoUrl &&
        !showCamera && (
          <button
            className="btn btn-secondary mb-3"
            onClick={startCamera}
            disabled={isLoading}
          >
            Bật camera
          </button>
        )}

      {showCamera && (
        <div className="camera-modal">
          <video ref={videoRef} autoPlay style={{ width: "100%" }} />
          <button className="btn btn-success mt-2" onClick={capturePhoto}>
            Chụp ảnh
          </button>
          <button className="btn btn-danger mt-2 ms-2" onClick={stopCamera}>
            Hủy
          </button>
        </div>
      )}

      {photoUrl && (
        <div className="photo-preview mb-3">
          <img
            src={photoUrl}
            alt="Ảnh xác thực"
            style={{ width: 200, borderRadius: 8 }}
          />
          <button
            className="btn btn-warning ms-2"
            onClick={() => {
              setPhotoUrl("");
              if (!showCamera) startCamera();
            }}
            disabled={isLoading}
          >
            Chụp lại
          </button>
        </div>
      )}

      {/* Thông báo trạng thái check-in sớm/muộn */}
      {checkInStatus && (
        <div className={`alert alert-info`}>
          Trạng thái check-in:{" "}
          {checkInStatus === "Early"
            ? "Sớm"
            : checkInStatus === "Late"
            ? "Muộn"
            : "Đúng giờ"}
        </div>
      )}

      {/* Nút check-in/out */}
      <div className="mb-3">
        <button
          className="btn btn-success me-2"
          onClick={handleCheckIn}
          disabled={canCheckOut || isLoading}
        >
          {isLoading ? "Đang xử lý..." : "Check-in"}
        </button>
        <button
          className="btn btn-danger"
          onClick={handleCheckOut}
          disabled={!canCheckOut || isLoading}
        >
          {isLoading ? "Đang xử lý..." : "Check-out"}
        </button>
      </div>

      {/* Thông báo trạng thái */}
      {lastCheckIn && (
        <div className="alert alert-info mb-3">
          {lastCheckIn.checkIn && (
            <div>
              Đã check-in lúc: {new Date(lastCheckIn.checkIn).toLocaleString()}
              {lastCheckIn.status && ` - Trạng thái: ${lastCheckIn.status}`}
            </div>
          )}
          {lastCheckIn.checkOut && (
            <div>
              Đã check-out lúc:{" "}
              {new Date(lastCheckIn.checkOut).toLocaleString()}
            </div>
          )}
        </div>
      )}

      {/* Thông báo chung */}
      {message && (
        <div
          className={`alert ${
            message.includes("thành công") ? "alert-success" : "alert-danger"
          } mt-3`}
        >
          {message}
        </div>
      )}

      {/* Lịch sử chấm công */}
      <div className="attendance-history">
        <h4>Lịch sử chấm công (7 ngày gần nhất)</h4>
        <div className="table-responsive">
          <table className="table table-striped">
            <thead>
              <tr>
                <th>Ngày</th>
                <th>Check-in</th>
                <th>Check-out</th>
                <th>Hình thức</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              {attendanceHistory.map((record) => (
                <tr key={record.id}>
                  <td>{new Date(record.checkIn).toLocaleDateString()}</td>
                  <td>{new Date(record.checkIn).toLocaleTimeString()}</td>
                  <td>
                    {record.checkOut
                      ? new Date(record.checkOut).toLocaleTimeString()
                      : "Chưa check-out"}
                  </td>
                  <td>{record.attendanceType}</td>
                  <td>{record.status || "---"}</td>
                </tr>
              ))}
              {attendanceHistory.length === 0 && (
                <tr>
                  <td colSpan="5" className="text-center">
                    Không có dữ liệu
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Canvas ẩn để chụp ảnh */}
      <canvas ref={canvasRef} style={{ display: "none" }} />
    </div>
  );
};

export default TimekeepingForm;
