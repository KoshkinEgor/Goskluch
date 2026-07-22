import { fetchAuthLogout } from "../repository/repository";
import { useNavigate } from "react-router";

export const Header = () => {

  const navigate = useNavigate();

  const handleAccountExit = () => {
    fetchAuthLogout()
    navigate("/")
  }

  return (
    
    <header className="bg-light border-bottom mb-3">
      <div className="container">
        <nav className="navbar">
          <div className="logo">
            <a className="text-decoration-none fs-4" href="/">Госключ Интеграция</a>
          </div>
          <div className="exit">
            <button className="btn btn-outline-danger" onClick={handleAccountExit}>Выйти</button>
          </div>
        </nav>
      </div>
    </header>
  );
};
