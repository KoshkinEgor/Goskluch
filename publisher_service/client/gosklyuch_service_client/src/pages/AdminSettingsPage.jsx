import { useEffect, useState } from "react";
import { Header } from "../components/Header";
import { AdminNavbar } from "../components/AdminNavbar";

import { fetchConfigSettings, fetchConfigSettingsPut } from "../repository/repository";

export const AdminSettingsPage = () => {
  const [settings, setSettings] = useState({});

  useEffect(() => {
    const getConfigSettings = async () => {
      var config = await fetchConfigSettings();
      setSettings(config);
    };

    getConfigSettings();
  }, []);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setSettings((prevSettings) => ({
      ...prevSettings,
      [name]: value,
    }));
  };

  const handleFormSubmit = (e) => {
    e.preventDefault()
    fetchConfigSettingsPut(settings)
  }

  return (
    <>
      <Header />
      <main>
        <div className="container">
          <AdminNavbar />

          <h2 className="mb-4">Настройки</h2>

          <form onSubmit={handleFormSubmit} encType="multipart/form-data">
            <div className="card mb-4">
              <div className="card-header">Данные организации</div>
              <div className="card-body">
                <div className="form-group mb-3">
                  <label className="col-form-label">Мнемоника внешней информационной системы </label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="mnemonics"
                      value={settings.mnemonics || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
                <div className="form-group mb-3">
                  <label className="col-form-label">Наименование услуги </label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="serviceName"
                      value={settings.serviceName || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
                <div className="form-group mb-3">
                  <label className="col-form-label">Наименование организации</label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="orgName"
                      value={settings.orgName || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
              </div>
            </div>

            <div className="card mb-4">
              <div className="card-header">Интеграция</div>
              <div className="card-body">
                <div className="form-group mb-3">
                  <label className="col-form-label">ServiceCode</label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="serviceCode"
                      value={settings.serviceCode || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
                <div className="form-group mb-3">
                  <label className="col-form-label">TargetCode</label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="targetCode"
                      value={settings.targetCode || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
                <div className="form-group mb-3">
                  <label className="col-form-label">Region</label>
                  <div>
                    <input
                      type="text"
                      className="form-control"
                      name="region"
                      value={settings.region || ""}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
              </div>
            </div>

            <button type="submit" className="btn btn-primary col-md-4 col-lg-2 mb-4">
              Сохранить
            </button>
          </form>
        </div>
      </main>
    </>
  );
};