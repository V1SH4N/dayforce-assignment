import React, { useState } from "react";
import "./InputForm.css";

interface Props {
  onSubmit: (jiraId: string) => void;
}

const InputForm: React.FC<Props> = ({ onSubmit }) => {

  const [jiraId, setJiraId] = useState("");

  return (
    <div className="inputCard">
      <h2 className="formTitle">Jira Test Case Generator</h2>

      <form
        className="form"
        onSubmit={(e) => {
          e.preventDefault();
          onSubmit(jiraId.trim().toUpperCase());
        }}
      >

        <label className="label">
          Jira ID:
        </label>

        <input
          type="text"
          required
          className="input"
          value={jiraId}
          onChange={(e) => setJiraId(e.target.value)}
        />

        <button type="submit" className="button">
          Generate Test Cases
        </button>

      </form>

    </div>
  );
};

export default InputForm;
