import { useState } from "react";
import { Header } from "../components/Header";
import { AdminNavbar } from "../components/AdminNavbar";

export const AdminSettingsPage = () => {
  return (
    <>
      <Header />
      <main>
        <div className="container">
          <AdminNavbar/>

          <h2 className="mb-4">Настройки</h2>

          <form action="" encType="multipart/form-data">
            <div className="card mb-4">
              <div className="card-header">Данные организации</div>
              <div className="card-body">
                <div class="form-group mb-3">
                  <label class="col-form-label">Мнемоника внешней информационной системы </label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={"MNSV03"}
                    />
                  </div>
                </div>
                <div class="form-group mb-3">
                  <label class="col-form-label">Наименование услуги </label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={"Отправка документов на подпись в «Госключ"}
                    />
                  </div>
                </div>
                <div class="form-group mb-3">
                  <label class="col-form-label">Наименование организации</label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={'ООО "СИМЭНЕРГО"'}
                    />
                  </div>
                </div>
              </div>
            </div>

            <div className="card mb-4">
              <div className="card-header">Интеграция</div>
              <div className="card-body">
                <div class="form-group mb-3">
                  <label class="col-form-label">ServiceCode</label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={"10000000374"}
                    />
                  </div>
                </div>
                <div class="form-group mb-3">
                  <label class="col-form-label">TargetCode</label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={"-10000000374"}
                    />
                  </div>
                </div>
                <div class="form-group mb-3">
                  <label class="col-form-label">Region</label>
                  <div>
                    <input
                      type="text"
                      class="form-control"
                      value={"45000000000"}
                    />
                  </div>
                </div>
              </div>
            </div>

            <button className="btn btn-primary col-md-4 col-lg-2 mb-4">
              Сохранить
            </button>
          </form>
        </div>
      </main>
    </>
  );
};
