import { useEffect, useState } from "react";
import { Header } from "../components/Header";
import { AdminNavbar } from "../components/AdminNavbar";
import { fetchUsers, fetchUserDelete, fetchUserCreate } from "../repository/repository";

export const AdminUsersPage = () => {
  const [showModal, setShowModal] = useState(false);
  const [users, setUsers] = useState([]);
  
  // Состояние для хранения данных формы
  const [formData, setFormData] = useState({
    name: "",
    login: "",
    password: "",
  });

  useEffect(() => {
    const getUsers = async () => {
      const fetchedUsers = await fetchUsers();
      setUsers(fetchedUsers);
    };
    getUsers();
  }, []);

  const handleUserDelete = async (userId) => {
    const res = await fetchUserDelete(userId);
    if (res?.id) {
      setUsers(users.filter((u) => u.id !== userId));
    }
  };

  // Обработчик создания пользователя
  const handleUserCreate = async (e) => {
    e.preventDefault();
    const res = await fetchUserCreate(formData);
    
    // Окно закроется только после успешного сохранения пользователя (наличие res.id)
    if (res?.id) {
      setUsers([...users, res]);
      setFormData({ name: "", login: "", password: "" });
      setShowModal(false); 
    }
  };

  // Обработчик закрытия модального окна со сбросом формы
  const handleCloseModal = () => {
    setShowModal(false);
    setFormData({ name: "", login: "", password: "" });
  };

  return (
    <>
      <Header />
      <main>
        <div className="container">
          <AdminNavbar />

          <h2 className="mb-4">Пользователи</h2>
          <div className="text-end">
            <button
              className="btn btn-primary col-md-4 col-lg-2 mb-4"
              onClick={() => setShowModal(true)}
            >
              Добавить пользователя +
            </button>
          </div>

          <div className="card mb-4">
            <div className="card-body">
              {users.length > 0 && (
                <table className="table">
                  <thead>
                    <tr>
                      <th scope="col">#</th>
                      <th scope="col">ФИО</th>
                      <th scope="col">Логин</th>
                      <th scope="col">Роль</th>
                      <th scope="col" className="col-sm-1"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((u, i) => (
                      <tr key={u.id}>
                        <td>{i + 1}</td>
                        <td>{u.name}</td>
                        <td>{u.login}</td>
                        <td>{u.role}</td>
                        <td>
                          <button
                            className="btn btn-danger py-0"
                            onClick={() => handleUserDelete(u.id)}
                          >
                            Удалить
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}

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
        </div>
      </main>

      {showModal && (
        <>
          <div
            className="modal fade show d-block"
            tabIndex="-1"
            role="dialog"
            style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
          >
            <div className="modal-dialog" role="document">
              <div className="modal-content">
                {/* Тег <form> теперь охватывает заголовок, тело и футер */}
                <form onSubmit={handleUserCreate}>
                  <div className="modal-header">
                    <h5 className="modal-title">Добавление пользователя</h5>
                    <button
                      type="button" // Указан type="button" во избежание отправки формы
                      className="btn-close"
                      onClick={handleCloseModal}
                      aria-label="Закрыть" // Убран некорректный атрибут required
                    ></button>
                  </div>
                  <div className="modal-body">
                    <div className="form-group mb-3">
                      <label className="mb-2">ФИО</label>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Введите ФИО"
                        value={formData.name}
                        required // Обязательное поле
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label className="mb-2">Логин</label>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Введите логин"
                        required // Обязательное поле
                        value={formData.login}
                        onChange={(e) => setFormData({ ...formData, login: e.target.value })}
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label className="mb-2">Пароль</label>
                      <input
                        type="password"
                        className="form-control"
                        placeholder="Введите пароль"
                        value={formData.password}
                        required // Обязательное поле
                        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                      />
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button 
                      type="button" // Изменено с "submit" на "button", чтобы форма не отправлялась при отмене
                      className="btn btn-secondary me-2" 
                      onClick={handleCloseModal}
                    >
                      Отмена
                    </button>
                    <button 
                      type="submit" // Изменено на "submit" для срабатывания onSubmit формы
                      className="btn btn-primary" 
                    >
                      Добавить
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
          <div className="modal-backdrop fade show"></div>
        </>
      )}
    </>
  );
};