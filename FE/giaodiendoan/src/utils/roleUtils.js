// roleUtils.js

// Lưu roles vào localStorage (nhận vào một hoặc nhiều role)
export function saveUserRoles(roles) {
  // roles phải là mảng
  if (!Array.isArray(roles)) {
    roles = [roles];
  }
  localStorage.setItem("userRoles", JSON.stringify(roles));
}

// Lấy roles từ localStorage trả về mảng (mặc định [] nếu không có)
export function getUserRoles() {
  const rolesStr = localStorage.getItem("userRoles");
  if (!rolesStr) return [];
  try {
    return JSON.parse(rolesStr);
  } catch {
    return [];
  }
}

// Kiểm tra xem user có role nào trong danh sách roles cần kiểm tra hay không
export function hasAnyRole(rolesToCheck = []) {
  const userRoles = getUserRoles();
  return userRoles.some((role) => rolesToCheck.includes(role));
}

// Kiểm tra user có role cụ thể không
export function hasRole(role) {
  const userRoles = getUserRoles();
  return userRoles.includes(role);
}

// Xóa roles (đăng xuất hoặc reset)
export function clearUserRoles() {
  localStorage.removeItem("userRoles");
}
