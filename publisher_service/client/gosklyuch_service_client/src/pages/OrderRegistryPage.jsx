import { Header } from "../components/Header";
import { useState, useEffect } from "react";
import { fetchOrders } from "../repository/repository";

export const OrderRegistryPage = () => {
  const [orders, setOrders] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");

  useEffect(() => {
    const getOrders = async () => {
      try {
        const fetchedOrders = await fetchOrders();
        setOrders(fetchedOrders);
      } catch (error) {
        console.error("Не удалось загрузить реестр запросов:", error);
      }
    };

    getOrders();
  }, []);

  return (
    <>
      <Header />

      <main>
        <div className="container">
          <nav className="navbar navbar-expand mb-4 px-0 mx-0">
            <div className="container-fluid justify-content-start px-0 mx-0">
              <ul className="navbar-nav me-auto gap-3 px-0 mx-0">
                <li className="nav-item">
                  <a href="/createesiaorder" className="nav-link px-0">
                    Создать запрос ЕСИА
                  </a>
                </li>

                <li className="nav-item">
                  <a href="/createsmevorder" className="nav-link px-0">
                    Создать запрос СМЭВ
                  </a>
                </li>
              </ul>
            </div>
          </nav>
          <h2 className="mb-4">Реестр запросов</h2>

          <div className="input-group mb-3">
            <input
              type="text"
              className="form-control"
              placeholder="СНИЛС или OID"
              aria-label="СНИЛС или OID"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
            <div className="input-group-append col-md-2">
              <button className="btn btn-outline-primary w-100" type="button">
                Найти
              </button>
            </div>
          </div>

          <table className="table">
            <thead>
              <tr>
                <th scope="col">#</th>
                <th scope="col">Дата заявки</th>
                <th scope="col">Получатель</th>
                <th scope="col">Статус</th>
                <th scope="col"></th>
              </tr>
            </thead>

            <tbody>
              {orders.map((o, i) => {
                const isSnils =
                  o.receiverSnils && o.receiverSnils.trim() !== "";
                const receiverLabel = isSnils ? "СНИЛС: " : "OID: ";
                const receiverValue = isSnils
                  ? o.receiverSnils
                  : o.receiverOid || "Не указан";

                return (
                  <tr key={o.id || i}>
                    <th scope="row">{i + 1}</th>
                    <td>
                      {o.createdDate
                        ? new Date(o.createdDate).toLocaleDateString("ru-RU")
                        : "—"}
                    </td>
                    <td>
                      <small className="text-secondary">{receiverLabel}</small>
                      {receiverValue}
                    </td>
                    <td className="text-secondary">{o.statusData.messageId}</td>
                    <td>
                      <a href={`/orders/${o.id}`}>Подробнее</a>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>

          <nav>
            <ul className="pagination">
              <li className="page-item">
                <a className="page-link" href="#" aria-label="Предыдущая">
                  <span aria-hidden="true">&laquo;</span>
                </a>
              </li>
              <li className="page-item active">
                <a className="page-link" href="#">
                  1
                </a>
              </li>
              <li className="page-item">
                <a className="page-link" href="#">
                  2
                </a>
              </li>
              <li className="page-item">
                <a className="page-link" href="#">
                  3
                </a>
              </li>
              <li className="page-item">
                <a className="page-link" href="#" aria-label="Следующая">
                  <span aria-hidden="true">&raquo;</span>
                </a>
              </li>
            </ul>
          </nav>
        </div>
      </main>
    </>
  );
};
