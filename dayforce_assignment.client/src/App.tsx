import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import InputPage from "./Pages/InputPage/InputPage";
import ResultsPage from "./Pages/ResultPage/ResultsPage";

const App: React.FC = () => {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<InputPage />} />
                <Route path="/results" element={<ResultsPage />} />
            </Routes>
        </Router>
    );
};

export default App;

