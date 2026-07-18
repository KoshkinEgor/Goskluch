// import "../styles/styles.css"
// import "../styles/utils.css"
// import "../styles/loginPage.css"

export const LoginPage = () => {
  return (
    <div className="d-flex justify-content-center align-items-center min-vh-100">
      <div className="card w-100" style={{ maxWidth: '400px' }}>
        <div className="card-body">
        <h2 className=" text-center mb-5">Госключ Интеграция</h2>
        <form action="" method="post">
          <div className="form-group mb-3">
            <label htmlFor="login">Логин</label>
            <input 
              className="form-control" 
              type="text" 
              id="login"
              name="login"
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