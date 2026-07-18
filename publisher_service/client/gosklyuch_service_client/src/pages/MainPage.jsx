import { Header } from "../components/Header";

export const MainPage = () => {
  return (
    <>
      <Header />

      <main>
        <div className="container">
          <nav className="navbar text-right mb-4">
            <a href="" className="nav-item">
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
            <div className="input-group-append">
              <button className="btn btn-outline-primary" type="button">
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
              <tr>
                <th scope="col">1</th>
                <td scope="col">11.02.2026</td>
                <td scope="col"><small className="text-secondary">СНИЛС:</small> 123-123-123 00</td>
                <td scope="col" className="text-success">Получена</td>
                <td scope="col">
                  <a href="">Подробнее</a>
                </td>
              </tr>
              <tr>
                <th scope="col">2</th>
                <td scope="col">11.02.2026</td>
                <td scope="col"><small className="text-secondary">СНИЛС:</small> 123-123-123 00</td>
                <td scope="col" className="text-warning">Отклонена</td>
                <td scope="col">
                  <a href="">Подробнее</a>
                </td>
              </tr>
              <tr>
                <th scope="col">3</th>
                <td scope="col">11.02.2026</td>
               <td scope="col"><small className="text-secondary">СНИЛС:</small> 123-123-123 00</td>
                <td scope="col" className="text-danger">Внутренная ошибка</td>
                <td scope="col">
                  <a href="">Подробнее</a>
                </td>
              </tr>
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
