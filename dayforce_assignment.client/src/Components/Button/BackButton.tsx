import React from "react";
import "./Button.css";

interface Props {
    onClick: () => void;
}

const BackButton: React.FC<Props> = ({ onClick }) => {
    return (
        <button className="backButton" onClick={onClick}>
            ← Back
        </button>
    );
};

export default BackButton;
