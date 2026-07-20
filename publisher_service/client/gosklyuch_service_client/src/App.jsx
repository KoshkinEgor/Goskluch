import { Routes, Route } from "react-router";

import { OrderRegistryPage  } from "./pages/OrderRegistryPage";
import { LoginPage } from "../src/pages/LoginPage";
import { OrderDetailsPage } from "./pages/OrderDetailsPage";
import { CreateOrderPage } from "./pages/CreateOrderPage";
import { AdminSettingsPage } from "./pages/AdminSettingsPage";
import { AdminUsersPage } from "./pages/AdminUsersPage";

function App() {
  
  return <Routes>
    <Route path="/login" element={<LoginPage/>}></Route>
    <Route path="/orders" element={<OrderRegistryPage/>}></Route>
    <Route path="/createorder" element={<CreateOrderPage/>}></Route>
    <Route path="/orders/:id" element={<OrderDetailsPage/>}></Route>
    <Route path="/admin/settings" element={<AdminSettingsPage/>}></Route>
    <Route path="/admin/users" element={<AdminUsersPage/>}></Route>

  </Routes>


}

export default App
