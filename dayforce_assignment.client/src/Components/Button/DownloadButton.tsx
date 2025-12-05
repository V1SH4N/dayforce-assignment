import React from "react";
import * as XLSX from "xlsx-js-style";
import type { TestCase } from "../../types/types";
import "./Button.css";

interface Props {
    data: TestCase[];
    filename?: string;
}

const MAX_COLUMN_WIDTH = 50; // Max characters per column

const DownloadButton: React.FC<Props> = ({ data, filename="testCases" }) => {
    const handleDownload = () => {
        if (!data || data.length === 0) return;

        const wsData = [
            ["Test Name", "Preconditions", "Steps", "Expected Result"],
            ...data.map(tc => [
                tc.testName,
                tc.preconditions,
                tc.steps,
                tc.expectedResult
            ])
        ];

        const ws = XLSX.utils.aoa_to_sheet(wsData);
        const range = XLSX.utils.decode_range(ws["!ref"]!);
        const borderStyle = { style: "thin", color: { rgb: "D1D1D1" } };

        for (let R = range.s.r; R <= range.e.r; ++R) {
            for (let C = range.s.c; C <= range.e.c; ++C) {
                const cellRef = XLSX.utils.encode_cell({ r: R, c: C });
                if (!ws[cellRef]) continue;

                ws[cellRef].s = {
                    font: { bold: R === 0 },
                    border: {
                        top: { borderStyle },
                        bottom: { borderStyle },
                        left: { borderStyle },
                        right: { borderStyle },
                    },
                    alignment: {
                        wrapText: true,
                        vertical: "top",
                        horizontal: "left"
                    }
                };
            }
        }

        // Auto column width, capped at MAX_COLUMN_WIDTH
        ws["!cols"] = wsData[0].map((_, colIndex) => {
            const maxLength = Math.max(
                ...wsData.map(row =>
                    row[colIndex] ? row[colIndex].toString().length : 10
                )
            );
            return { wch: Math.min(maxLength + 2, MAX_COLUMN_WIDTH) };
        });

        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Test Cases");
        XLSX.writeFile(wb, filename);
    };

    return (
        <button onClick={handleDownload} className="downloadButton">
            Download Excel
        </button>
    );
};

export default DownloadButton;

