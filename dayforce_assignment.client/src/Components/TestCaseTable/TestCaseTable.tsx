import React from "react";
import type { TestCase } from "../../types/types";
import DownloadButton from "../Button/DownloadButton";
import "./TestCaseTable.css";

interface Props {
    testCases: TestCase[];
    jiraKey?: string;
}

const TestCaseTable: React.FC<Props> = ({ testCases, jiraKey }) => {
    return (
        <div className="testCaseContainer">
            <div>
                <div className="testCaseTitleContainer">
                    <h2>Test Cases</h2>
                </div>
                <div className="downloadButtonWrapper">
                    <DownloadButton
                        data={testCases}
                        filename={`${jiraKey ?? "test-cases"}.xlsx`}
                    />
                </div>
            </div>
           

            <table>
                <thead>
                    <tr>
                        <th scope="col">Test Name</th>
                        <th scope="col">Preconditions</th>
                        <th scope="col">Steps</th>
                        <th scope="col">Expected Result</th>
                    </tr>
                </thead>

                <tbody>
                    {testCases.map((tc) => (
                        <tr key={tc.testName}>
                            <td>{tc.testName}</td>
                            <td>{tc.preconditions}</td>
                            <td>{tc.steps}</td>
                            <td>{tc.expectedResult}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

        </div>
    );
};

export default TestCaseTable;



