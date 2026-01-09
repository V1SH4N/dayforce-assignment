import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import "./ResultPage.css";
import IssueContextOverview from "../../Components/IssueContextOverview/IssueContextOverview";
import TestCaseEventListener from "../../Components/TestCaseTable/TestCaseEventListener";
import BackButton from "../../Components/Button/BackButton";
import Error from "../../Components/Error/Error";


interface ErrorInfo {
  title: string;
  detail: string;
  statusCode: number;
}


const ResultPage: React.FC = () => {

  const location = useLocation();
  const navigate = useNavigate();
  const jiraKey = (location.state as { jiraKey?: string })?.jiraKey;

  const [error, setError] = useState<ErrorInfo | null>(null);
  const [eventSource, setEventSource] = useState<EventSource | null>(null);

  useEffect(() => {
    if (!jiraKey) {
      navigate("/", { replace: true });
    }
  }, [jiraKey, navigate]);

  useEffect(() => {
    const source = new EventSource(
      `https://localhost:7091/api/TestCaseGenerator/generate/stream?jiraKey=${jiraKey}`
    );
    setEventSource(source);

    const handleError = (e: MessageEvent) => {
      const data = JSON.parse(e.data);
      setError({
        title: data.Title,
        detail: data.Detail,
        statusCode: data.StatusCode,
      });
      source.close();
    };

    const handleComplete = () => {
      source.close();
    };

    source.addEventListener("ErrorEvent", handleError);
    source.addEventListener("requestComplete", handleComplete);

    return () => {
      source.removeEventListener("ErrorEvent", handleError);
      source.removeEventListener("complete", handleComplete);
      source.close();
    };
  }, [jiraKey]);



  if (error) {
    return <Error
        title={error.title}
        detail={error.detail}
        statusCode={error.statusCode}
    />;
  };

  return (
    <div className="resultContainer">
      <BackButton onClick={() => navigate("/")} />
      <IssueContextOverview eventSource={eventSource} />
      <TestCaseEventListener eventSource={eventSource} jiraKey={jiraKey!} />
    </div>
  );


};

export default ResultPage;
