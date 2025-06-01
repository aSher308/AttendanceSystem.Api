import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  getAllLocations,
  deleteLocation,
  createLocation,
  updateLocation,
} from "../utils/locationService";
import { toast } from "react-toastify";

const LocationForm = () => {
  // State cho danh sách locations và loading
  const [locations, setLocations] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  // State cho form
  const [formData, setFormData] = useState({
    name: "",
    latitude: 0,
    longitude: 0,
    radiusInMeters: 100,
    isDefault: false,
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Lấy danh sách locations khi component mount
  useEffect(() => {
    fetchLocations();
  }, []);

  // Reset form khi selectedLocation thay đổi
  useEffect(() => {
    if (selectedLocation) {
      setFormData({
        id: selectedLocation.id,
        name: selectedLocation.name,
        latitude: selectedLocation.latitude,
        longitude: selectedLocation.longitude,
        radiusInMeters: selectedLocation.radiusInMeters,
        isDefault: selectedLocation.isDefault,
      });
    } else {
      setFormData({
        name: "",
        latitude: 0,
        longitude: 0,
        radiusInMeters: 100,
        isDefault: false,
      });
    }
  }, [selectedLocation]);

  // Hàm lấy danh sách locations
  const fetchLocations = async () => {
    try {
      setIsLoading(true);
      const data = await getAllLocations();
      setLocations(data);
    } catch (error) {
      toast.error("Lỗi khi tải danh sách vị trí");
      console.error("Error fetching locations:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Hàm mở form thêm mới
  const handleAddLocation = () => {
    setSelectedLocation(null);
    setShowForm(true);
  };

  // Hàm mở form chỉnh sửa
  const handleEditLocation = (location) => {
    setSelectedLocation(location);
    setShowForm(true);
  };

  // Hàm xóa location
  const handleDeleteLocation = async (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa vị trí này?")) {
      try {
        await deleteLocation(id);
        toast.success("Xóa vị trí thành công");
        fetchLocations();
      } catch (error) {
        toast.error("Lỗi khi xóa vị trí");
        console.error("Error deleting location:", error);
      }
    }
  };

  // Hàm đóng form
  const handleFormClose = () => {
    setShowForm(false);
    fetchLocations();
  };

  // Hàm xử lý thay đổi input
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === "checkbox" ? checked : value,
    });
  };

  // Hàm validate form
  const validate = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = "Tên vị trí là bắt buộc";
    }

    if (
      isNaN(formData.latitude) ||
      formData.latitude < -90 ||
      formData.latitude > 90
    ) {
      newErrors.latitude = "Vĩ độ phải từ -90 đến 90";
    }

    if (
      isNaN(formData.longitude) ||
      formData.longitude < -180 ||
      formData.longitude > 180
    ) {
      newErrors.longitude = "Kinh độ phải từ -180 đến 180";
    }

    if (isNaN(formData.radiusInMeters)) {
      newErrors.radiusInMeters = "Bán kính phải là số";
    } else if (formData.radiusInMeters <= 0) {
      newErrors.radiusInMeters = "Bán kính phải lớn hơn 0";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Hàm submit form
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validate()) return;

    setIsSubmitting(true);

    try {
      if (formData.id) {
        await updateLocation(formData);
        toast.success("Cập nhật vị trí thành công");
      } else {
        await createLocation(formData);
        toast.success("Thêm vị trí thành công");
      }
      handleFormClose();
    } catch (error) {
      toast.error(error.response?.data?.message || "Có lỗi xảy ra");
      console.error("Error saving location:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container-fluid">
      {/* Phần danh sách locations */}
      <div className="row mb-4">
        <div className="col-md-6">
          <h2>Quản lý Vị trí</h2>
        </div>
        <div className="col-md-6 text-end">
          <button className="btn btn-primary" onClick={handleAddLocation}>
            <i className="fas fa-plus me-2"></i>Thêm Vị trí
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="table table-striped table-hover">
                <thead>
                  <tr>
                    <th>STT</th>
                    <th>Tên vị trí</th>
                    <th>Tọa độ</th>
                    <th>Bán kính (m)</th>
                    <th>Mặc định</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {locations.map((location, index) => (
                    <tr key={location.id}>
                      <td>{index + 1}</td>
                      <td>{location.name}</td>
                      <td>
                        {location.latitude}, {location.longitude}
                      </td>
                      <td>{location.radiusInMeters}</td>
                      <td>
                        {location.isDefault ? (
                          <span className="badge bg-success">Mặc định</span>
                        ) : null}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-warning me-2"
                          onClick={() => handleEditLocation(location)}
                        >
                          <i className="fas fa-edit"></i>
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => handleDeleteLocation(location.id)}
                        >
                          <i className="fas fa-trash"></i>
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Form thêm/sửa location (hiển thị dạng modal) */}
      {showForm && (
        <div
          className="modal fade show"
          style={{ display: "block", backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {formData.id ? "Cập nhật Vị trí" : "Thêm Vị trí Mới"}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={handleFormClose}
                ></button>
              </div>
              <div className="modal-body">
                <form onSubmit={handleSubmit}>
                  <div className="mb-3">
                    <label className="form-label">
                      Tên vị trí <span className="text-danger">*</span>
                    </label>
                    <input
                      type="text"
                      className={`form-control ${
                        errors.name ? "is-invalid" : ""
                      }`}
                      name="name"
                      value={formData.name}
                      onChange={handleChange}
                    />
                    {errors.name && (
                      <div className="invalid-feedback">{errors.name}</div>
                    )}
                  </div>

                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">
                        Vĩ độ <span className="text-danger">*</span>
                      </label>
                      <input
                        type="number"
                        className={`form-control ${
                          errors.latitude ? "is-invalid" : ""
                        }`}
                        name="latitude"
                        value={formData.latitude}
                        onChange={handleChange}
                        step="any"
                      />
                      {errors.latitude && (
                        <div className="invalid-feedback">
                          {errors.latitude}
                        </div>
                      )}
                    </div>

                    <div className="col-md-6 mb-3">
                      <label className="form-label">
                        Kinh độ <span className="text-danger">*</span>
                      </label>
                      <input
                        type="number"
                        className={`form-control ${
                          errors.longitude ? "is-invalid" : ""
                        }`}
                        name="longitude"
                        value={formData.longitude}
                        onChange={handleChange}
                        step="any"
                      />
                      {errors.longitude && (
                        <div className="invalid-feedback">
                          {errors.longitude}
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="mb-3">
                    <label className="form-label">
                      Bán kính (mét) <span className="text-danger">*</span>
                    </label>
                    <input
                      type="number"
                      className={`form-control ${
                        errors.radiusInMeters ? "is-invalid" : ""
                      }`}
                      name="radiusInMeters"
                      value={formData.radiusInMeters}
                      onChange={handleChange}
                    />
                    {errors.radiusInMeters && (
                      <div className="invalid-feedback">
                        {errors.radiusInMeters}
                      </div>
                    )}
                  </div>

                  <div className="mb-3 form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      name="isDefault"
                      checked={formData.isDefault}
                      onChange={handleChange}
                      id="isDefaultCheck"
                    />
                    <label
                      className="form-check-label"
                      htmlFor="isDefaultCheck"
                    >
                      Đặt làm vị trí mặc định
                    </label>
                  </div>

                  <div className="modal-footer">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={handleFormClose}
                    >
                      Hủy
                    </button>
                    <button
                      type="submit"
                      className="btn btn-primary"
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? (
                        <span
                          className="spinner-border spinner-border-sm me-1"
                          role="status"
                          aria-hidden="true"
                        ></span>
                      ) : null}
                      {formData.id ? "Cập nhật" : "Thêm mới"}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default LocationForm;
