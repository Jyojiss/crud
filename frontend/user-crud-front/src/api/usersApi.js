import axiosClient from "./axiosClient";

export const getUsersRequest = async ({ search = "", page = 1, size = 10 }) => {
  const response = await axiosClient.get("/users", {
    params: {
      search,
      page,
      size,
    },
  });

  return response.data;
};

export const getUserByIdRequest = async (id) => {
  const response = await axiosClient.get(`/users/${id}`);
  return response.data;
};

export const createUserRequest = async (userData) => {
  const response = await axiosClient.post("/users", userData);
  return response.data;
};

export const updateUserRequest = async (id, userData) => {
  const response = await axiosClient.put(`/users/${id}`, userData);
  return response.data;
};

export const deleteUserRequest = async (id) => {
  const response = await axiosClient.delete(`/users/${id}`);
  return response.data;
};