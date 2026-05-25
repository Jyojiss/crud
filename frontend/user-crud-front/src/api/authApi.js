import axiosClient from "./axiosClient";

export const loginRequest = async (credentials) => {
  const response = await axiosClient.post("/auth/login", credentials);
  return response.data;
};

export const registerRequest = async (userData) => {
  const response = await axiosClient.post("/auth/register", userData);
  return response.data;
};