import { Header } from "../components/Header";

import { useState, useEffect } from "react";
import { fetchOrders } from "../repository/repository";

export const OrderRegistryPage = () => {
  const [orders, setOrders] = useState([]);

  useEffect(() => {
    const getOrders = async () => {
      const fetchedOrders = await fetchOrders();
      setOrders(fetchedOrders);
    };

    getOrders();
  }, []);

  return (
    <>
      <Header />

      <main>
        <div className="container">
          <nav className="navbar text-right mb-4">
            <a href="/createorder" className="nav-item">
              + Создать запрос
            </a>
          </nav>
          <h2 className="mb-4">Реестр запросов</h2>
          <div className="input-group mb-3">
            <input
              type="text"
              className="form-control"
              placeholder="СНИЛС или OID"
              aria-label="СНИЛС или OID"
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
                return (
                  <tr key={i}>
                    <th scope="col">{i + 1}</th>
                    <td scope="col">{
                    
                    
                    new Date(o.createdDate).toLocaleDateString('ru-Ru')
                    
                    
                    
                    
                    }</td>
                    <td scope="col">
                      <small className="text-secondary">
                        {o.receiverIdType == "snils" ? "СНИЛС: " : "OID: "}
                      </small>
                      {o.receiverId}
                    </td>
                    <td scope="col" className="text-success">
                      Получена
                    </td>
                    {/* <td scope="col" className="text-warning">
                  Отклонена
                </td> */}
                    {/* <td scope="col" className="text-danger">
                  Внутренная ошибка
                </td> */}
                    <td scope="col">
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
