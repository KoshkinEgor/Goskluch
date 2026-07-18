import { useState } from "react";
import { Header } from "../components/Header";
import { AdminNavbar } from "../components/AdminNavbar";

export const AdminUsersPage = () => {
  // Состояние для управления видимостью модального окна
  const [showModal, setShowModal] = useState(false);

  return (
    <>
      <Header />
      <main>
        <div className="container">
          <AdminNavbar />

          <h2 className="mb-4">Пользователи</h2>
          <div className="text-end">
            {/* Убираем data-bs-toggle и data-bs-target, используем onClick */}
            <button
              className="btn btn-primary col-md-4 col-lg-2 mb-4"
              onClick={() => setShowModal(true)}
            >
              Добавить пользователя +
            </button>
          </div>

          <form action="" encType="multipart/form-data">
            <div className="card mb-4">
              <div className="card-body">
                <table className="table">
                  <thead>
                    <tr>
                      <th scope="col">#</th>
                      <th scope="col">ФИО</th>
                      <th scope="col">Логин</th>
                      <th scope="col" className="col-sm-1"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {/* Пример строки таблицы */}
                    <tr>
                      <td>1</td>
                      <td>Петров Петр Петрович</td>
                      <td>PetrovPetr@mail.ru</td>
                      <td>
                        <button className="btn btn-danger py-0">Удалить</button>
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
                    <li className="page-item">
                      <a className="page-link active" href="#">
                        1
                      </a>
                    </li>
                    <li className="page-item">
                      <a className="page-link" href="#">
                        2
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
            </div>
          </form>
        </div>
      </main>

      {/* Модальное окно, управляемое через состояние React */}
      {showModal && (
        <>
          <div
            className="modal fade show d-block"
            tabIndex="-1"
            role="dialog"
            style={{ backgroundColor: "rgba(0,0,0,0.5)" }} // Опционально: затемнение фона
          >
            <div className="modal-dialog" role="document">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Добавление пользователя</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setShowModal(false)}
                    aria-label="Закрыть"
                  ></button>
                </div>
                <div className="modal-body">
                  <form>
                    <div className="form-group mb-3">
                      <label className="mb-2">ФИО</label>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Введите ФИО"
                      />
                     
                    </div>
                    <div className="form-group mb-3">
                      <label className="mb-2">Логин</label>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Введите логин"
                      />
                     
                    </div>
                    <div className="form-group mb-3">
                      <label className="mb-2">Пароль</label>
                      <input
                        type="password"
                        className="form-control"
                        placeholder="Введите парль"
                      />
                    </div>
                   
                  </form>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-primary">
                    Сохранить изменения
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div className="modal-backdrop fade show"></div>
        </>
      )}
    </>
  );
};
