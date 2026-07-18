import { useState } from "react";
import { Header } from "../components/Header";

export const CreateOrderPage = () => {
  const [files, setFiles] = useState([]);
  const MAX_FILES = 15;

  // Функция форматирования размера
  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Б';
    const k = 1024;
    const sizes = ['Б', 'Кб', 'Мб', 'Гб'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleFileChange = (e) => {
    const selectedFiles = Array.from(e.target.files || []);
    setFiles(prevFiles => {
      const newFiles = [...prevFiles, ...selectedFiles];
      return newFiles.slice(0, MAX_FILES);
    });
    e.target.value = ''; // Сброс значения input для возможности повторного выбора тех же файлов
  };

  const handleRemoveFile = (indexToRemove) => {
    setFiles(prevFiles => prevFiles.filter((_, index) => index !== indexToRemove));
  };

  const totalSize = files.reduce((acc, file) => acc + file.size, 0);

  return (
    <>
      <Header />
      <main>
        <div className="container">
          <nav className="navbar text-right mb-4">
            <a href="/orders" className="nav-item">‹ Главная</a>
          </nav>
          <h2 className="mb-4">Детали запроса</h2>
          
          {/* Важно: добавлен encType для корректной отправки файлов */}
          <form action="" encType="multipart/form-data">
            
            {/* ... Блок "Данные запроса" остался без изменений ... */}
            <div className="card mb-4">
              <div className="card-header">Данные запроса</div>
              <div className="card-body">
                <div className="card-title">Получатель</div>
                <div className="row g-3 align-items-center mb-3">
                  <div className="col-md-3">
                    <select className="form-select">
                      <option value="snils">СНИЛС</option>
                      <option value="oid">OID</option>
                    </select>
                  </div>
                  <div className="col-md-9">
                    <input type="text" className="form-control" placeholder="Введите идентификатор получателя" />
                  </div>
                </div>

                <div className="card-title">Тип подписи получателя</div>
                <div className="row g-3 align-items-center mb-3">
                  <div className="col-md-3">
                    <select className="form-select">
                      <option value="kap">КЭП</option>
                      <option value="nap">НЭП</option>
                    </select>
                  </div>
                </div>

                <div className="row g-3 align-items-center">
                  <div className="form-group w-100">
                    <label htmlFor="orderDescriptionTextarea" className="mb-2">Описание запроса</label>
                    <textarea className="form-control" id="orderDescriptionTextarea" rows="3"></textarea>
                  </div>
                </div>
              </div>
            </div>

            {/* --- Переработанный блок "Пакет документов" --- */}
            <div className="card mb-4">
              <div className="card-header">Пакет документов</div>
              <div className="card-body">
                
                {/* Кастомная кнопка загрузки */}
                <div className="position-relative d-inline-block mb-3">
                  {/* Визуальная кнопка */}
                  <button 
                    type="button" 
                    className="btn btn-outline-secondary" 
                    disabled={files.length >= MAX_FILES}
                  >
                    Выбрать файлы
                  </button>
                  
                  {/* Скрытый input, перекрывающий кнопку */}
                  <input
                    type="file"
                    multiple
                    onChange={handleFileChange}
                    disabled={files.length >= MAX_FILES}
                    style={{
                      position: 'absolute',
                      top: 0,
                      left: 0,
                      width: '100%',
                      height: '100%',
                      opacity: 0,
                      cursor: files.length >= MAX_FILES ? 'not-allowed' : 'pointer'
                    }}
                  />
                </div>

                {files.length >= MAX_FILES && (
                  <div className="text-warning small mb-2">
                    Достигнут лимит файлов ({MAX_FILES})
                  </div>
                )}

                {/* Таблица загруженных файлов */}
                {files.length > 0 && (
                  <table className="table table-sm mt-2">
                    <tbody>
                      {files.map((file, index) => (
                        <tr key={index}>
                          <th scope="row" style={{ width: '40px' }}>{index + 1}</th>
                          <td>{file.name}</td>
                          <td className="text-end">
                            <button 
                              type="button"
                              className="btn btn-link text-danger p-0 text-decoration-none"
                              onClick={() => handleRemoveFile(index)}
                              title="Удалить файл"
                            >
                              Удалить X
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
                
                {/* Статистика */}
                <div className="d-flex justify-content-end flex-column text-muted small mt-2">
                  <span>{formatFileSize(totalSize)}</span>
                  <span>{files.length}/{MAX_FILES}</span>
                </div>

              </div>
            </div>

            <button className="btn btn-primary w-100 mb-4">Отправить</button>
          </form>
        </div>
      </main>
    </>
  );
};