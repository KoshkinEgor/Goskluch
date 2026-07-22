import { useEffect, useState } from "react";
import { fetchAuth } from "../repository/repository";
import { useNavigate } from "react-router";

export const LoginPage = () => {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  const handleAuth = async (e) => {
    e.preventDefault(); // Предотвращаем стандартную перезагрузку страницы

    const auth = async () => {
      var result = await fetchAuth(login, password);
      if (result?.userRole == null) {
        console.log("Не авторизован");
        navigate("/login");
      }
      if (result?.userRole == "user") {
        navigate("/orders");
      }
      if (result?.userRole == "admin") {
        navigate("/admin/users");
      }
    };

    auth()
  };

  return (
    <div className="d-flex justify-content-center align-items-center min-vh-100">
      <div className="card w-100" style={{ maxWidth: "400px" }}>
        <div className="card-body">
          <h2 className="text-center mb-5">Госключ Интеграция</h2>

          {/* Добавлен обработчик onSubmit и удалены action/method */}
          <form onSubmit={handleAuth}>
            <div className="form-group mb-3">
              <label htmlFor="login">Логин</label>
              <input
                className="form-control"
                type="text"
                id="login"
                name="login"
                value={login}
                onChange={(e) => setLogin(e.target.value)} // Корректное обновление состояния
                required
              />
            </div>
            <div className="form-group mb-5">
              <label htmlFor="password">Пароль</label>
              <input
                className="form-control"
                type="password"
                id="password"
                name="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)} // Корректное обновление состояния
                required
              />
            </div>
            <button
              className="btn btn-primary w-100 mb-5"
              id="btn-submit"
              type="submit"
            >
              Войти
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
