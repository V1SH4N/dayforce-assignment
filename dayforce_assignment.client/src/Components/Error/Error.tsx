import React from "react";
import { useNavigate } from "react-router-dom";
import BackButton from "../Button/BackButton";
import "./Error.css";


interface ErrorProps {
  title: string;
  detail: string;
  statusCode?: number;
}

const Error: React.FC<ErrorProps> = ({ title, detail, statusCode }) => {
  const navigate = useNavigate();

  return (
    <div className="errorPage">
      <h1 className="errorTitle">{`Error ${statusCode}: ${title}`}</h1>
      <p className="errorDetail">{detail}</p>
      <BackButton onClick={() => navigate("/")} />
    </div>
  );
};

export default Error;
