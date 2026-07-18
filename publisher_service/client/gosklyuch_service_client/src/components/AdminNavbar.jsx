export const AdminNavbar = () => {
  return (
    <>
      <nav className="navbar navbar-expand">
        <div id="navbarNav">
          <ul className="navbar-nav">
            <li className="nav-item">
              <a className="nav-link" href="/admin/users">
                Пользователи
              </a>
            </li>
            <li className="nav-item active">
              <a className="nav-link" href="/admin/settings">
                Настройки
              </a>
            </li>
          </ul>
        </div>
      </nav>
    </>
  );
};
