import { useState, useEffect } from "react";
import { useParams } from "react-router";

import { Header } from "../components/Header";
import { fetchOrder } from "../repository/repository";

export const OrderDetailsPage = () => {
  const { id } = useParams();
  const [order, setOrder] = useState(null);

  useEffect(() => {
    const getOrder = async (id) => {
      const fetchedOrder = await fetchOrder(id);
      setOrder(fetchedOrder);
    };

    getOrder(id);
  }, [id]);

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
              ‹ Реестр запросов
            </a>
          </nav>
          <h2 className="mb-4">Детали запроса</h2>

          <div className="card mb-4">
            <div className="card-header">Данные запроса</div>
            <div className="card-body">
              <p className="card-text">
                <b>Создан:</b>{" "}
                {new Date(order.createdDate).toLocaleDateString("ru-RU")}
              </p>
              <p className="card-text">
                {order.receiverIdType === "snils" ? (
                  <>
                    <b>СНИЛС: </b>
                    {order.receiverId}
                  </>
                ) : (
                  <>
                    <b>OID: </b>
                    {order.receiverId}
                  </>
                )}
              </p>

              <p className="card-text">
                <b>Отправитель:</b> {order.userName}
              </p>
              <p className="card-text">
                <b>Описание:</b> {order.description}
              </p>
            </div>
          </div>
          
          <OrderMessageCardSucceed />
          {/* <OrderMessageCardDeclined/> */}
          {/* <OrderMessageCardInternalError /> */}
          
          <div className="card mb-4">
            <div className="card-header">Состав пакета документов</div>
            <div className="card-body">
              <table className="table">
                <tbody>
                  {order.documentsPack && order.documentsPack.length > 0 ? (
                    order.documentsPack.map((d, i) => (
                      <tr key={i}>
                        <th scope="row">{i + 1}</th>
                        <td>{d.name || "Документ"}</td>
                        <td>
                          <a href="">Скачать &darr;</a>
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

const OrderMessageCardSucceed = () => {
  return (
    <div className="card mb-4">
      <div className="card-header">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Подписан
          </div>
          <button className="btn btn-primary">
            Скачать подписанные документы &darr;
          </button>
        </div>
        <div>
          <p className="card-text text-secondary">
            Документы подписаны получателем и доступны для скачивания.
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageCardDeclined = () => {
  return (
    <div className="card mb-4">
      <div className="card-header">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Отклонен
          </div>
          <button className="btn btn-primary">
            Повторить отправку &#8635;
          </button>
        </div>
        <div>
          <p className="card-text text-secondary">
            Подписание документов было отклонено получателем. При необходимости
            повторите запрос.
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageCardInternalError = () => {
  return (
    <div className="card mb-4">
      <div className="card-header">Статус запроса</div>
      <div className="card-body">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Внутренняя ошибка
          </div>
        </div>
        <div>
          <p className="card-text text-secondary">
            Произошла внутренняя ошибка при отправке запроса. Проверьте данные
            получателя и сформируйте запрос повторно.
          </p>
        </div>
      </div>
    </div>
  );
};