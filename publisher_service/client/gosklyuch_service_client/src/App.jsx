import { Routes, Route } from "react-router";

import { MainPage  } from "./pages/MainPage";
import { LoginPage } from "../src/pages/LoginPage";
import { OrderDetailsPage } from "./pages/OrderDetailsPage";
import { CreateOrderPage } from "./pages/CreateOrderPage";


function App() {
  
  return <Routes>
    <Route path="/login" element={<LoginPage/>}></Route>
    <Route index path="/orders" element={<MainPage/>}></Route>
    <Route index path="/createorder" element={<CreateOrderPage/>}></Route>
    <Route index path="/orderdetails" element={<OrderDetailsPage/>}></Route>
  </Routes>


}

export default App
