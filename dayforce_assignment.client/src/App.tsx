import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import InputPage from "./Pages/InputPage/InputPage";
import ResultPage from "./Pages/ResultPage/ResultPage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<InputPage />} />
        <Route path="/testCase" element={<ResultPage />} />
      </Routes>
    </Router>
  );
}

export default App;
