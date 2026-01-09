import React from "react";
import * as XLSX from "xlsx-js-style";
import "./Button.css";
import type { TestCase } from "../../CommonInterfaces/Interfaces";

interface Props {
  data: TestCase[];
  filename?: string;
}

const MAX_COLUMN_WIDTH = 50; 

const DownloadButton: React.FC<Props> = ({ data, filename = "testCases.xlsx" }) => {
  const handleDownload = () => {
    if (!data || data.length === 0) return;

    // worksheet data
    const wsData = [
      ["Test Name", "Preconditions", "Steps", "Expected Result"],
      ...data.map(tc => [tc.testName, tc.preconditions, tc.steps, tc.expectedResult]),
    ];

    const ws = XLSX.utils.aoa_to_sheet(wsData);
    const range = XLSX.utils.decode_range(ws["!ref"]!);
    const thinBorder = { style: "thin", color: { rgb: "D1D1D1" } };

    // Apply styling for each cell
    for (let R = range.s.r; R <= range.e.r; ++R) {
      for (let C = range.s.c; C <= range.e.c; ++C) {
        const cellRef = XLSX.utils.encode_cell({ r: R, c: C });
        const cell = ws[cellRef];
        if (!cell) continue;

        cell.s = {
          font: { bold: R === 0 }, 
          alignment: { wrapText: true, vertical: "top", horizontal: "left" },
          border: {
            top: thinBorder,
            bottom: thinBorder,
            left: thinBorder,
            right: thinBorder,
          },
        };
      }
    }

    // Column width capped at MAX_COLUMN_WIDTH
    ws["!cols"] = wsData[0].map((_, colIndex) => {
      const maxLength = Math.max(
        ...wsData.map(row => (row[colIndex] ? row[colIndex].toString().length : 10))
      );
      return { wch: Math.min(maxLength + 2, MAX_COLUMN_WIDTH) };
    });


    // Create workbook and download
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Test Cases");
    XLSX.writeFile(wb, filename.endsWith(".xlsx") ? filename : `${filename}.xlsx`);

  };

  return (
    <button onClick={handleDownload} className="downloadButton">
      Download Excel
    </button>
  );
};

export default DownloadButton;
