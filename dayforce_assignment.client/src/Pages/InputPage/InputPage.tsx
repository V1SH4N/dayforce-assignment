import React, { useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import "./InputPage.css";
import JiraInputFormCard from "../../Components/JiraIssueFormCard/JiraIssueFormCard";
import type { ApiResponse } from "../../types/types";

const InputPage: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const fetchTestCases = useCallback(async (jiraKey: string) => {
        setLoading(true);
        setError(null);

        const startTime = performance.now();

        try {
            const res = await fetch(
                `https://localhost:7091/api/TestCaseGenerator/testCases?jiraId=${encodeURIComponent(jiraKey)}`
            );

            if (import.meta.env.DEV) {
                console.log(`[Timer] API fetch completed in ${(performance.now() - startTime).toFixed(2)} ms`);
            }

            if (!res.ok) {
                let msg = `HTTP error ${res.status}`;
                try {
                    const problem = await res.json();
                    msg = problem?.detail || problem?.title || msg;
                } catch { }
                setError(msg);
                return;
            }

            const data: ApiResponse = await res.json();

            if (data.status === "success") {
                navigate("/results", {
                    state: {
                        jiraKey,
                        jiraTitle: data.jiraTitle,
                        confluencePages: data.confluencePagesConsidered,
                        testCases: data.testCases,
                    }
                });
            } else {
                setError("Insufficient information provided by Jira issue to generate test cases");
            }
        } catch (err) {
            const msg = err instanceof Error ? err.message : "Network error";
            setError(msg);
        } finally {
            setLoading(false);
        }
    }, [navigate]);

    return (
        <div className="formContainer">
            <JiraInputFormCard
                loading={loading}
                error={error}
                onSubmit={fetchTestCases}
            />
        </div>
    );
};

export default InputPage;



