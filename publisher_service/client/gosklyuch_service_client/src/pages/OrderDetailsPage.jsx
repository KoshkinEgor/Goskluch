import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router";

import { Header } from "../components/Header";
import { 
  fetchOrder, 
  fetchCOrderRetry, 
  downloadSignedDocuments // Добавлен импорт новой функции
} from "../repository/repository";
import { DocumentLoadLink } from "../components/DocumentLoadLink";

export const OrderDetailsPage = () => {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleRetry = (orderId) => {
    fetchCOrderRetry(orderId);
    alert("Отправлен повторный запрос");
    navigate("/orders");
  };

  useEffect(() => {
    const getOrder = async (id) => {
      try {
        const fetchedOrder = await fetchOrder(id);
        setOrder(fetchedOrder);
      } catch (err) {
        console.error("Не удалось загрузить детали запроса:", err);
        setError("Ошибка загрузки данных. Попробуйте обновить страницу.");
      }
    };

    getOrder(id);
  }, [id]);

  if (error) {
    return (
      <>
        <Header />
        <main>
          <div className="container">
            <p className="text-danger">{error}</p>
            <a href="/orders" className="btn btn-outline-secondary">
              Вернуться в реестр
            </a>
          </div>
        </main>
      </>
    );
  }

  if (!order) {
    return (
      <>
        <Header />
        <main>
          <div className="container">
            <p>Загрузка...</p>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <Header />
      <main>
        <div className="container">
          <nav className="navbar text-right mb-4">
            <a href="/orders" className="nav-item">
              Реестр запросов
            </a>
          </nav>
          <h2 className="mb-4">Детали запроса</h2>

          <div className="card mb-4">
            <div className="card-header">Данные запроса</div>
            <div className="card-body">
              <p className="card-text">
                <b>Создан:</b>{" "}
                {order.createdDate
                  ? new Date(order.createdDate).toLocaleDateString("ru-RU")
                  : "—"}
              </p>

              <p className="card-text">
                {order.receiverSnils ? (
                  <>
                    <b>СНИЛС: </b>
                    {order.receiverSnils}
                  </>
                ) : (
                  <>
                    <b>OID: </b>
                    {order.receiverOid || "Не указан"}
                  </>
                )}
              </p>

              <p className="card-text">
                <b>Отправитель:</b> {order.userName || "Не указан"}
              </p>
              <p className="card-text">
                <b>Описание:</b> {order.description || "—"}
              </p>
            </div>
          </div>

          {order.statusData?.orderStatusId === 17 && <OrderMessageInQueueCard statusData={order.statusData} />}
          
          {/* Передаем orderId в компонент успешного статуса */}
          {order.statusData?.orderStatusId === 3 && <OrderMessageDoneCard statusData={order.statusData} orderId={order.id} />} 
          
          {order.statusData?.orderStatusId === 4 && <OrderMessageDeclinedCard statusData={order.statusData} orderId={order.id} handleRetry={handleRetry} />}
          {order.statusData?.orderStatusId === 5 && <OrderMessageInternalErrorCard statusData={order.statusData} orderId={order.id} handleRetry={handleRetry} />}

          <div className="card mb-4">
            <div className="card-header">Состав пакета документов</div>
            <div className="card-body">
              <table className="table">
                <tbody>
                  {order.documentsPack && order.documentsPack.length > 0 ? (
                    order.documentsPack.map((d, i) => (
                      <tr key={d.id || i}>
                        <th scope="row">{i + 1}</th>
                        <td>{d.name}</td>
                        <td>
                          <DocumentLoadLink res={d.localName} />
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="3" className="text-center">
                        Документы отсутствуют
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </main>
    </>
  );
};

const OrderMessageDoneCard = ({ statusData, orderId }) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleDownload = async () => {
    setIsLoading(true);
    try {
      const response = await downloadSignedDocuments(orderId);
      
      // Попытка извлечь имя файла из заголовка Content-Disposition
      const contentDisposition = response.headers["content-disposition"];
      let fileName = `order_${orderId}_signed.zip`;
      
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/);
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1];
        }
      }

      // Создание Blob-объекта и временной ссылки для инициации скачивания
      const blob = new Blob([response.data], { type: "application/zip" });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
      
      // Очистка
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Ошибка скачивания:", error);
      alert("Не удалось скачать архив. Возможно, подписи еще не сформированы или произошла ошибка сервера.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="card mb-4 border-success">
      <div className="card-header bg-success text-white">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> {statusData.messageId || "Документы подписаны"}
          </div>
          <button 
            className="btn btn-primary" 
            onClick={handleDownload}
            disabled={isLoading}
          >
            {isLoading ? "Загрузка..." : "Скачать подписанные документы ↓"}
          </button>
        </div>
        <div>
          <p className="card-text text-secondary">
            {statusData.messageAnnotation || "Пользователь успешно подписал документы через МП «Госключ»."}
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageDraftCard = ({statusData}) => {
  return (
    <div className="card mb-4">
      <div className="card-header">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> {statusData.messageId}
          </div>
        </div>
        <div>
          <p className="card-text text-secondary">
              {statusData.messageAnnotation}
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageInQueueCard = ({statusData}) => {
  return (
    <div className="card mb-4 border-info">
      <div className="card-header bg-info text-white">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> {statusData.messageId}
          </div>
        </div>
        <div>
          <p className="card-text text-secondary">
              {statusData.messageAnnotation}
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageDeclinedCard = ({statusData, orderId, handleRetry}) => {
  return (
    <div className="card mb-4 border-warning">
      <div className="card-header bg-warning text-dark">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> {statusData.messageId}
          </div>
          <button className="btn btn-primary" onClick={() => handleRetry(orderId)}>
            Повторить отправку &#8635;
          </button>
        </div>
        <div>
          <p className="card-text text-secondary">
             {statusData.messageAnnotation}
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageInternalErrorCard = ({statusData, orderId, handleRetry}) => {
  return (
    <div className="card mb-4 border-danger">
      <div className="card-header bg-danger text-white">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> {statusData.messageId}
          </div>
           <button className="btn btn-primary" onClick={() => handleRetry(orderId)}>
            Повторить отправку &#8635;
          </button>
        </div>
        <div>
          <p className="card-text text-secondary">
            {statusData.messageAnnotation}
          </p>
        </div>
      </div>
    </div>
  );
};

