import React, { useState, useEffect } from "react";
import { API_URL } from "../config";
import "../styles/style.css";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";

// Fix icon mặc định Leaflet (nếu bạn chưa fix)
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl:
    "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

const TimekeepingForm = () => {
  const [userId] = useState(1);
  const [locationName] = useState("");
  const [deviceInfo, setDeviceInfo] = useState("");
  const [latitude, setLatitude] = useState(null);
  const [longitude, setLongitude] = useState(null);
  const [photoUrl] = useState("");
  const [message, setMessage] = useState("");
  const [attendanceType, setAttendanceType] = useState("Onsite");
  const [showMap, setShowMap] = useState(false);

  useEffect(() => {
    setDeviceInfo(navigator.userAgent);
  }, []);

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
          setMessage("Không thể lấy vị trí của bạn.");
        }
      );
    } else {
      setMessage("Trình duyệt không hỗ trợ lấy vị trí.");
    }
  };

  const handleCheckIn = async () => {
    if (latitude == null || longitude == null) {
      setMessage("Vui lòng lấy vị trí trước khi chấm công.");
      return;
    }
    try {
      // Chuyển string thành số enum tương ứng (0 = Onsite, 1 = Remote)
      const attendanceTypeValue = attendanceType === "Onsite" ? 0 : 1;

      const response = await fetch(`${API_URL}/Attendance/check-in`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId,
          locationName,
          deviceInfo,
          latitude,
          longitude,
          photoUrl,
          attendanceType: attendanceTypeValue,
        }),
      });

      const data = await response.text();
      setMessage(data);
    } catch (err) {
      console.error(err);
      setMessage("Lỗi kết nối server khi chấm công vào");
    }
  };

  const handleCheckOut = async () => {
    if (latitude == null || longitude == null) {
      setMessage("Vui lòng lấy vị trí trước khi chấm công.");
      return;
    }
    try {
      const response = await fetch(`${API_URL}/Attendance/check-out`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId,
          deviceInfo,
          latitude,
          longitude,
          photoUrl,
        }),
      });

      const data = await response.text();
      setMessage(data);
    } catch (err) {
      console.error(err);
      setMessage("Lỗi kết nối server khi chấm công ra");
    }
  };

  return (
    <div className="timekeeping-container">
      <h2>Chấm công</h2>
      <div className="mb-3">
        <label>Hình thức làm việc:</label>
        <select
          className="form-control"
          value={attendanceType}
          onChange={(e) => setAttendanceType(e.target.value)}
        >
          <option value="Onsite">Làm tại văn phòng</option>
          <option value="Remote">Làm từ xa</option>
        </select>
      </div>

      <button className="btn btn-primary mb-3" onClick={getLocation}>
        Lấy vị trí
      </button>

      {latitude && longitude && (
        <>
          <p>
            Vị trí hiện tại: {latitude.toFixed(6)}, {longitude.toFixed(6)}
          </p>

          {showMap && (
            <div
              style={{ height: "300px", width: "100%", marginBottom: "1rem" }}
            >
              <MapContainer
                center={[latitude, longitude]}
                zoom={16}
                style={{ height: "100%", width: "100%" }}
              >
                <TileLayer
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                />
                <Marker position={[latitude, longitude]}>
                  <Popup>Vị trí của bạn</Popup>
                </Marker>
              </MapContainer>
            </div>
          )}
        </>
      )}

      <button className="btn btn-success me-2" onClick={handleCheckIn}>
        Check-in
      </button>
      <button className="btn btn-danger" onClick={handleCheckOut}>
        Check-out
      </button>

      {message && <div className="alert alert-info mt-3">{message}</div>}
    </div>
  );
};

export default TimekeepingForm;
