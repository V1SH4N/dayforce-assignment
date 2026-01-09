import React, { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import "./InputPage.css";
import InputForm from "../../Components/InputForm/InputForm";

const InputPage: React.FC = () => {
  const navigate = useNavigate();

  const onSubmit = useCallback(
    (jiraKey: string) => {
      navigate("/testCase", { state: { jiraKey } });
    },
    [navigate]
  );

  return (
    <div className="formContainer">
      <InputForm onSubmit={onSubmit} />
    </div>
  );
};

export default InputPage;
