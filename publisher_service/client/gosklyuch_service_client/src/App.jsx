import { Routes, Route, Navigate, Outlet, useLocation } from "react-router";
import { OrderRegistryPage } from "./pages/OrderRegistryPage";
import { LoginPage } from "../src/pages/LoginPage";
import { OrderDetailsPage } from "./pages/OrderDetailsPage";
import { CreateOrderPage } from "./pages/CreateOrderPage";
import { AdminSettingsPage } from "./pages/AdminSettingsPage";
import { AdminUsersPage } from "./pages/AdminUsersPage";
import { useEffect, useState } from "react";

function App() {


  return (
    <Routes>
     
     
      <Route>
        <Route path="/orders" element={<OrderRegistryPage />} />
        <Route path="/createorder" element={<CreateOrderPage />} />
        <Route path="/orders/:id" element={<OrderDetailsPage />} />
      </Route>

      
      <Route>
        <Route path="admin/settings" element={<AdminSettingsPage />} />
        <Route path="admin/users" element={<AdminUsersPage />} />
      </Route>

      
      <Route path="/login" element={<LoginPage />} />
      <Route path="*" element={<Navigate to="/login" replace />} />

    </Routes>
  );
}

const getCookie = (name) => {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop().split(";").shift();
  return null;
};

export default App;
