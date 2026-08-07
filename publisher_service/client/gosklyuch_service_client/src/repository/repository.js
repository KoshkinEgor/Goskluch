import axios from "axios";

axios.defaults.baseURL = "http://localhost:5197";
axios.defaults.withCredentials = true;

export async function fetchOrders() {
  try {
    const response = await axios.get("/orders");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные заказов:", error.message);
    return [];
  }
}

export async function fetchOrder(id) {
  try {
    const response = await axios.get(`/orders/${id}`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные заказов:", error.message);
    return [];
  }
}

export async function fetchEsiaOrderCreate(orderData, files) {
  try {
    const formData = new FormData();

    if (orderData.receiverIdType === "snils") {
      formData.append("ReceiverSnils", orderData.receiverId || "");
    } else if (orderData.receiverIdType === "oid") {
      formData.append("ReceiverOid", orderData.receiverId || "");
    }

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

    const response = await axios.post("/esiaorders/", formData);

    return response.data;
  } catch (error) {
    console.error("Не удалось создать заказ:", error.message);
    throw error;
  }
}

export async function fetchSmevOrderCreate(orderData, files) {
  try {
    const formData = new FormData();

    formData.append("ReceiverSnils", orderData.receiverId || "");
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

    const response = await axios.post("/smevorders/", formData);

    return response.data;
  } catch (error) {
    console.error("Не удалось создать заказ:", error.message);
    throw error;
  }
}

export async function fetchConfigSettings() {
  try {
    const response = await axios.get("/configsettings");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}

export async function fetchCOrderRetry(id) {
  try {
    const response = await axios.post(`/orders/retry/${id}`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}

export async function fetchConfigSettingsPut(settings) {
  try {
    const response = await axios.put("/configsettings", settings);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}

export async function fetchUsers() {
  try {
    const response = await axios.get("/users");
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователей:", error.message);
    return [];
  }
}

export async function fetchUserDelete(Id) {
  try {
    const response = await axios.delete(`/users/${Id}`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователей:", error.message);
    return [];
  }
}

export async function fetchUserCreate(userData) {
  try {
    const response = await axios.post(`/users/`, userData, {
      withCredentials: true,
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
    const response = await axios.post(`/auth/`, {
      login: login,
      password: password,
    });
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователя:", error.message);
    return "";
  }
}

export async function fetchAuthLogout() {
  try {
    const response = await axios.delete(`/auth/`);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные пользователя:", error.message);
    return "";
  }
}


export async function downloadSignedDocuments(orderId) {
  try {
    const response = await axios.get(`/orders/download-signed/${orderId}`, {
      responseType: "blob", // Указываем, что ожидаем бинарные данные (zip-архив)
    });
    return response;
  } catch (error) {
    console.error("Не удалось скачать подписанные документы:", error.message);
    throw error;
  }
}