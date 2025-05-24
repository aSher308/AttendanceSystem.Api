import React, { useState, useEffect, useRef } from "react";
import { API_URL } from "../config";
import "../styles/style.css";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";

// Fix icon mặc định Leaflet
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
  const [photoUrl, setPhotoUrl] = useState("");
  const [message, setMessage] = useState("");
  const [workLocation, setWorkLocation] = useState("Onsite");
  const [checkInMethod, setCheckInMethod] = useState("GPS");
  const [showMap, setShowMap] = useState(false);
  const [showCamera, setShowCamera] = useState(false);
  const [cameraMode, setCameraMode] = useState(null); // 'qr' hoặc 'face'
  const videoRef = useRef(null);
  const canvasRef = useRef(null);

  useEffect(() => {
    setDeviceInfo(navigator.userAgent);
  }, []);

  useEffect(() => {
    if (workLocation === "Remote") {
      setCheckInMethod("FaceRecognition");
    } else {
      setCheckInMethod("GPS");
    }
  }, [workLocation]);

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

  const startCamera = async (mode) => {
    try {
      setCameraMode(mode);
      setShowCamera(true);

      const stream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: mode === "face" ? "user" : "environment",
        },
      });

      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
    } catch (err) {
      console.error("Lỗi khi truy cập camera:", err);
      setMessage(
        "Không thể truy cập camera. Vui lòng kiểm tra quyền truy cập."
      );
    }
  };

  const stopCamera = () => {
    if (videoRef.current && videoRef.current.srcObject) {
      videoRef.current.srcObject.getTracks().forEach((track) => track.stop());
    }
    setShowCamera(false);
    setCameraMode(null);
  };

  const capturePhoto = () => {
    if (videoRef.current && canvasRef.current) {
      const context = canvasRef.current.getContext("2d");
      canvasRef.current.width = videoRef.current.videoWidth;
      canvasRef.current.height = videoRef.current.videoHeight;
      context.drawImage(videoRef.current, 0, 0);

      const photoDataUrl = canvasRef.current.toDataURL("image/png");
      setPhotoUrl(photoDataUrl);
      setMessage("Đã chụp ảnh thành công!");
      stopCamera();

      // Ở đây có thể thêm logic xử lý QR code hoặc nhận diện khuôn mặt
      if (cameraMode === "qr") {
        // Xử lý QR code
        // processQRCode(photoDataUrl);
      } else {
        // Xử lý nhận diện khuôn mặt
        // processFaceRecognition(photoDataUrl);
      }
    }
  };

  const mapToAttendanceType = () => {
    if (workLocation === "Remote") {
      return 2; // FaceRecognition cho làm từ xa
    }

    // Cho làm tại văn phòng
    switch (checkInMethod) {
      case "QR":
        return 1;
      case "GPS":
        return 0;
      default:
        return 0;
    }
  };

  const handleCheckIn = async () => {
    if (checkInMethod === "GPS" && (latitude == null || longitude == null)) {
      setMessage("Vui lòng lấy vị trí trước khi chấm công.");
      return;
    }

    if ((checkInMethod === "QR" || workLocation === "Remote") && !photoUrl) {
      setMessage("Vui lòng chụp ảnh xác thực trước khi chấm công.");
      return;
    }

    // Chuyển latitude và longitude về number hoặc null (backend có thể xử lý nullable)
    const lat = latitude !== null ? Number(latitude) : null;
    const lng = longitude !== null ? Number(longitude) : null;

    // Kiểm tra giá trị number hợp lệ
    if (checkInMethod === "GPS" && (isNaN(lat) || isNaN(lng))) {
      setMessage("Vị trí không hợp lệ.");
      return;
    }

    const dataToSend = {
      userId,
      locationName: locationName || null, // gửi null nếu rỗng
      deviceInfo,
      latitude: lat,
      longitude: lng,
      photoUrl: photoUrl || null,
      attendanceType: mapToAttendanceType(),
    };

    console.log("Sending data:", dataToSend);

    try {
      const response = await fetch(`${API_URL}/Attendance/check-in`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dataToSend),
      });

      const responseText = await response.text();

      if (!response.ok) {
        // Hiển thị lỗi chi tiết từ backend
        setMessage(`Lỗi server: ${response.status} - ${responseText}`);
        return;
      }

      setMessage(responseText);
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
          value={workLocation}
          onChange={(e) => setWorkLocation(e.target.value)}
        >
          <option value="Onsite">Làm tại văn phòng</option>
          <option value="Remote">Làm từ xa</option>
        </select>
      </div>

      <div className="mb-3">
        <label>Phương thức chấm công:</label>
        <select
          className="form-control"
          value={checkInMethod}
          onChange={(e) => setCheckInMethod(e.target.value)}
          disabled={workLocation === "Remote"}
        >
          {workLocation === "Onsite" ? (
            <>
              <option value="GPS">GPS</option>
              <option value="QR">Quét QR Code</option>
            </>
          ) : (
            <option value="FaceRecognition">Nhận diện khuôn mặt</option>
          )}
        </select>
      </div>

      {checkInMethod === "GPS" && (
        <button className="btn btn-primary mb-3" onClick={getLocation}>
          Lấy vị trí
        </button>
      )}

      {(checkInMethod === "QR" || workLocation === "Remote") && (
        <div className="mb-3">
          <button
            className="btn btn-info mb-2"
            onClick={() => startCamera(checkInMethod === "QR" ? "qr" : "face")}
          >
            {checkInMethod === "QR" ? "Quét QR Code" : "Nhận diện khuôn mặt"}
          </button>
          {photoUrl && (
            <div className="mt-2">
              <p>Ảnh đã chụp:</p>
              <img
                src={photoUrl}
                alt="Xác thực"
                style={{ maxWidth: "100%", maxHeight: "200px" }}
              />
            </div>
          )}
        </div>
      )}

      {showCamera && (
        <div className="camera-modal">
          <div className="camera-content">
            <video
              ref={videoRef}
              autoPlay
              playsInline
              className="camera-view"
            />
            <canvas ref={canvasRef} style={{ display: "none" }} />
            <div className="camera-controls">
              <button className="btn btn-success me-2" onClick={capturePhoto}>
                Chụp ảnh
              </button>
              <button className="btn btn-danger" onClick={stopCamera}>
                Đóng camera
              </button>
            </div>
          </div>
        </div>
      )}

      {latitude && longitude && showMap && checkInMethod === "GPS" && (
        <>
          <p>
            Vị trí hiện tại: {latitude.toFixed(6)}, {longitude.toFixed(6)}
          </p>
          <div style={{ height: "300px", width: "100%", marginBottom: "1rem" }}>
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
