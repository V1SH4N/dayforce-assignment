import React from "react";
import type { ConfluencePage } from "../../types/types";
import "./GeneralInfoList.css";

interface Props {
    jiraKey: string;
    jiraTitle: string;
    confluencePages: ConfluencePage[];
}

const GeneralInfoList: React.FC<Props> = ({ jiraKey, jiraTitle, confluencePages }) => {
    return (
        <div className="generalInfoContainer">
            <a
                className="title"
                target="_blank"
                rel="noopener noreferrer"
                href={`https://dayforce.atlassian.net/browse/${jiraKey}`}
            >
                {jiraKey}: {jiraTitle}
            </a>

            {confluencePages.length > 0 && (
                <>
                    <p className="subtitle">Confluence pages considered:</p>
                    <ul className="list">
                        {confluencePages.map((page) => (
                            <li key={page.id} className="listItem">
                                <a
                                    href={`https://dayforce.atlassian.net/wiki/pages/viewpage.action?pageId=${page.id}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="link"
                                >
                                    {page.title}
                                </a>
                            </li>
                        ))}
                    </ul>
                </>
            )}
        </div>
    );
};

export default GeneralInfoList;

