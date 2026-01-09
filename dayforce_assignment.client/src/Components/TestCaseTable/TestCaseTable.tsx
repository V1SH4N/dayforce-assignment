import React from "react";
import type { TestCase } from "../../types/types";
import DownloadButton from "../Button/DownloadButton";
import "./TestCaseTable.css";

interface Props {
  testCases: TestCase[];
  jiraKey?: string;
  status?: "pending" | "inProgress" | "finished";
}

const TestCaseTable: React.FC<Props> = ({ testCases, jiraKey, status }) => {
  const showTitle = status !== "pending";
  const showDownload = status === "finished";

  const skeletonRowsCount = status === "pending" ? 2 : status === "inProgress" ? 1 : 0;

  const renderSkeletonRow = (idx: number) => (
    <tr key={`skeleton-${idx}`} className="skeletonRow">
      {Array.from({ length: 4 }).map((_, cellIdx) => (
        <td key={cellIdx} className="skeletonCell">
        <div className="skeletonCellContent"></div>
        </td>
      ))}
    </tr>
  );

  return (
    <div className="testCaseContainer">
      <div className="testCaseHeader">
        <div className="testCaseTitleContainer">
          {showTitle ? <h2>Test Cases</h2> : <div className="skeletonLine titleSkeleton"></div>}
        </div>
      </div>

    <table>
      {(status === "inProgress" || status === "finished") && (
        <thead>
          <tr>
           <th scope="col">Test Name</th>
            <th scope="col">Preconditions</th>
            <th scope="col">Steps</th>
            <th scope="col">Expected Result</th>
          </tr>
        </thead>
      )}
       

        <tbody>
          {testCases.map((tc, idx) => (
            <tr key={`${tc.testName}-${idx}`}>
              <td>{tc.testName}</td>
              <td>{tc.preconditions}</td>
              <td>{tc.steps}</td>
              <td>{tc.expectedResult}</td>
            </tr>
          ))}

          {Array.from({ length: skeletonRowsCount }).map((_, idx) => renderSkeletonRow(idx))}
        </tbody>
      </table>

      <div className="downloadButtonWrapper">
        {showDownload ? (
          <DownloadButton data={testCases} filename={`${jiraKey ?? "test-cases"}.xlsx`} />
        ) : (
          <div className="skeletonLine downloadSkeleton"></div>
        )}
      </div>
    </div>
  );
};

export default TestCaseTable;
