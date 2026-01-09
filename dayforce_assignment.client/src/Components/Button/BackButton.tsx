import React from "react";
import { AiOutlineArrowLeft } from "react-icons/ai";
import "./Button.css";

interface Props {
    onClick: () => void;
}

const BackButton: React.FC<Props> = ({ onClick }) => {
    return (
        <button className="backButton" onClick={onClick}>
            <AiOutlineArrowLeft />
        </button>
    );
};

export default BackButton;
