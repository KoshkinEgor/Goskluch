import axios from "axios";

axios.defaults.withCredentials = true;

export async function fetchOrders() {
  try {
    const response = await axios.get("http://localhost:5197/orders");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные заказов:", error.message);
    return [];
  }
}

export async function fetchOrder(id) {
  try {
    const response = await axios.get(`http://localhost:5197/orders/${id}`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные заказов:", error.message);
    return [];
  }
}

export async function fetchOrderCreate(orderData, files) {
  try {
    const formData = new FormData();

    formData.append("ReceiverIdType", orderData.receiverIdType || "");
    formData.append("ReceiverId", orderData.receiverId || "");
    formData.append("SignatureType", orderData.signatureType || "");
    formData.append("Description", orderData.description || "");

    if (files) {
      const fileArray =
        files instanceof FileList
          ? Array.from(files)
          : Array.isArray(files)
            ? files
            : [files];

      fileArray.forEach((file) => {
        formData.append("DocumentsPack", file);
      });
    }

    const response = await axios.post(
      "http://localhost:5197/orders/",
      formData,
    );

    return response.data;
  } catch (error) {
    console.error("Не удалось создать заказ:", error.message);
    throw error;
  }
}

export async function fetchConfigSettings() {
  try {
    const response = await axios.get("http://localhost:5197/configsettings");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}

export async function fetchConfigSettingsPut(settings) {
  try {
    const response = await axios.put(
      "http://localhost:5197/configsettings",
      settings,
    );
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}

export async function fetchUsers() {
  try {
    const response = await axios.get("http://localhost:5197/users");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователей:", error.message);
    return [];
  }
}

export async function fetchUserDelete(Id) {
  try {
    const response = await axios.delete(`http://localhost:5197/users/${Id}`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователей:", error.message);
    return [];
  }
}

export async function fetchUserCreate(userData) {
  try {
    const response = await axios.post(`http://localhost:5197/users/`, userData, {
      withCredentials: true
    });
    console.log(response.data);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователей:", error.message);
    return [];
  }
}

export async function fetchAuth(login, password) {
  try {
    const response = await axios.post(`http://localhost:5197/auth/`, {
      login: login,
      password: password,
    });
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователя:", error.message);
    return "";
  }
}
