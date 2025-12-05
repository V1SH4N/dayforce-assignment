import React, { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import GeneralInfoList from "../../Components/GeneralInfoList/GeneralInfoList";
import TestCaseTable from "../../Components/TestCaseTable/TestCaseTable";
import BackButton from "../../Components/Button/BackButton";
import type { ConfluencePage, TestCase } from "../../types/types";
import "./ResultPage.css";

interface LocationState {
    jiraKey: string;
    jiraTitle: string;
    confluencePages: ConfluencePage[];
    testCases: TestCase[];
}

const ResultsPage: React.FC = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const state = location.state as LocationState | undefined;

    useEffect(() => {
        if (!state) {
            navigate("/", { replace: true });
        }
    }, [state, navigate]);

    if (!state) return null;

    const { jiraKey, jiraTitle, confluencePages, testCases } = state;

    return (
        <main className="resultContainer">
            <BackButton onClick={() => navigate("/")} />

            <GeneralInfoList
                jiraKey={jiraKey}
                jiraTitle={jiraTitle}
                confluencePages={confluencePages}
            />
            <TestCaseTable testCases={testCases} jiraKey={jiraKey} />
        </main>
    );
};

export default ResultsPage;

