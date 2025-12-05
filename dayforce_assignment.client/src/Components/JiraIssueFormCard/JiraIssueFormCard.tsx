import React, { useState } from "react";
import "./JiraIssueFormCard.css";

interface Props {
    loading: boolean;
    error: string | null;
    onSubmit: (jiraId: string) => void;
}

const JiraInputFormCard: React.FC<Props> = ({ loading, error, onSubmit }) => {
    const [jiraId, setJiraId] = useState("");

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSubmit(jiraId.trim().toUpperCase());
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setJiraId(e.target.value);
    };

    return (
        <div className="card">
            <h2 className="title">Jira Test Case Generator</h2>

            <form onSubmit={handleSubmit} className="form">
                <div className="formSection">
                    <label htmlFor="jira-id" className="label">Jira ID:</label>

                    <input
                        id="jira-id"
                        type="text"
                        value={jiraId}
                        onChange={handleChange}
                        required
                        className="input"
                    />
                </div>
                
                <div className="formSection">
                    <button
                        type="submit"
                        disabled={loading}
                        className="button"
                    >
                        {loading ? (
                            <span className="spinner"></span>
                        ) : (
                            "Generate Test Cases"
                        )}
                    </button>

                    <p className="error">{error}</p>
                </div>
                
            </form>
        </div>
    );
};

export default JiraInputFormCard;

