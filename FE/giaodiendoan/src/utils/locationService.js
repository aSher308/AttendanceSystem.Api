import axiosInstance from "../utils/axiosInstance"; // đường dẫn đúng với project bạn

const API_URL = "/location";

export const getAllLocations = async () => {
  const response = await axiosInstance.get(API_URL);
  return response.data;
};

export const getLocationById = async (id) => {
  const response = await axiosInstance.get(`${API_URL}/${id}`);
  return response.data;
};

export const createLocation = async (locationData) => {
  const response = await axiosInstance.post(API_URL, locationData);
  return response.data;
};

export const updateLocation = async (locationData) => {
  const response = await axiosInstance.put(
    `${API_URL}/${locationData.id}`,
    locationData
  );
  return response.data;
};

export const deleteLocation = async (id) => {
  await axiosInstance.delete(`${API_URL}/${id}`);
};
