import { Header } from "../components/Header";

export const OrderDetailsPage = () => {
  return (
    <>
      <Header />
      <main>
        <div className="container">
          <nav className="navbar text-right mb-4">
            <a href="/orders" className="nav-item">
              ‹ Главная
            </a>
          </nav>
          <h2 className="mb-4">Детали запроса</h2>

          <div class="card mb-4">
            <div class="card-header">Данные запроса</div>
            <div class="card-body">
              <p class="card-text">
                <b>Создан:</b> 11.02.2026
              </p>
              <p class="card-text">
                <b>СНИЛС:</b> 123-123-123 00
              </p>
              <p class="card-text">
                <b>Описание:</b> Запрос на подписание документов
              </p>
            </div>
          </div>
          {/* <OrderMessageCardSucceed /> */}
          {/* <OrderMessageCardDeclined/> */}
          <OrderMessageCardInternalError />
          <div class="card">
            <div class="card-header">Состав пакета документов</div>
            <div class="card-body">
              <table class="table">
                <tbody>
                  <tr>
                    <th scope="row">1</th>
                    <td>Договок_оказания_услуг_№32.pdf</td>
                    <td>
                      <a href="">Скачать &darr;</a>
                    </td>
                  </tr>
                  <tr>
                    <th scope="row">2</th>
                    <td>Приложеие_№1_Техническое задание.tiff</td>
                    <td>
                      <a href="">Скачать &darr;</a>
                    </td>
                  </tr>
                  <tr>
                    <th scope="row">3</th>
                    <td>Скан_паспорта_заявителя.jpg</td>
                    <td>
                      <a href="">Скачать &darr;</a>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </main>
    </>
  );
};

const OrderMessageCardSucceed = () => {
  return (
    <div class="card mb-4">
      <div class="card-header">Статус запроса</div>
      <div class="card-body ">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Подписан
          </div>
          <button className="btn btn-primary">
            Скачать подписанные документы &darr;
          </button>
        </div>
        <div>
          <p class="card-text text-secondary">
            Документы подписаны получателем и доступны для скачивания.
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageCardDeclined = () => {
  return (
    <div class="card mb-4">
      <div class="card-header">Статус запроса</div>
      <div class="card-body ">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Отклонен
          </div>
          <button className="btn btn-primary">
            Повторить отправку &#8635;
          </button>
        </div>
        <div>
          <p class="card-text text-secondary">
            Подписание документов было отклонено получателем. При необходимости
            повторите запрос.
          </p>
        </div>
      </div>
    </div>
  );
};

const OrderMessageCardInternalError = () => {
  return (
    <div class="card mb-4">
      <div class="card-header">Статус запроса</div>
      <div class="card-body ">
        <div className="d-flex align-items-center justify-content-between mb-2">
          <div>
            <b>Статус:</b> Внутренняя ошибка
          </div>
        </div>
        <div>
          <p class="card-text text-secondary">
            Произошла внутренняя ошибка при отправке запроса. Проверьте данные
            получателя и сформируйте запрос повторно.
          </p>
        </div>
      </div>
    </div>
  );
};
