import axios from "axios";

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
    const response = await axios.put("http://localhost:5197/configsettings", settings);
    return response.data;
  } catch (error) {
    console.error("Не удалось получить данные конфигурации:", error.message);
    return [];
  }
}
