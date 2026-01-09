import React, { useEffect, useRef, useState } from "react";
import TestCaseTable from "../TestCaseTable/TestCaseTable";
import type { TestCase } from "../../CommonInterfaces/Interfaces";
import "./TestCaseEventListener.css"

interface TestCaseEventListenerProps {
  eventSource: EventSource;
  jiraKey?: string;
}

const TestCaseEventListener: React.FC<TestCaseEventListenerProps> = ({
  eventSource,
  jiraKey,
}) => {
  const [testCases, setTestCases] = useState<TestCase[]>([]);
  const [status, setStatus] = useState<"pending" | "inProgress" | "finished">("pending");
  const [insufficientData, setInsufficientData] = useState(false);


  const userHasScrolledRef = useRef(false);
  const isAutoScrollingRef = useRef(false);

  // SSE EVENT HANDLING
  useEffect(() => {
    if (!eventSource) return;

    const handleGenerated = (event: MessageEvent) => {
      try {
        const data: TestCase = JSON.parse(event.data);

        // Check if empty object -> insufficient data
        if (Object.keys(data).length === 0) {
          setInsufficientData(true);
          setStatus("finished");
          return;
        }

        setTestCases((prev) => {
          if (prev.length === 0) {
            setStatus("inProgress");
          }
          return [...prev, data];
        });
      } catch (err) {
        console.error("Failed to parse testCaseGenerated event:", err);
      }
    };

    const handleFinished = () => setStatus("finished");

    eventSource.addEventListener("testCaseGenerated", handleGenerated);
    eventSource.addEventListener("testCasesFinished", handleFinished);

    return () => eventSource.close();
  }, [eventSource]);

  // User scroll detection
  useEffect(() => {
    const onScroll = () => {
      if (isAutoScrollingRef.current) return;

      const threshold = 50; // px from bottom
      const distanceFromBottom =
        document.documentElement.scrollHeight -
        window.innerHeight -
        window.scrollY;

      // User scrolled up -> disable auto-scroll
      if (distanceFromBottom > threshold) {
        userHasScrolledRef.current = true;
      }

      // User returned to bottom -> re-enable auto-scroll
      if (distanceFromBottom <= threshold) {
        userHasScrolledRef.current = false;
      }
    };

    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  // Auto scroll on new data
  useEffect(() => {
    if (userHasScrolledRef.current) return;

    isAutoScrollingRef.current = true;

    window.scrollTo({
      top: document.body.scrollHeight,
      behavior: "smooth",
    });

    // Allow user scroll detection again
    setTimeout(() => {
      isAutoScrollingRef.current = false;
    }, 300);
  }, [testCases]);

  return (
    <div className="testCaseEventListenerContainer">
      {insufficientData ? (
        <div className="insufficientDataText">
          Insufficient data to generate test cases.
        </div>
      ) : (
        <TestCaseTable testCases={testCases} jiraKey={jiraKey} status={status} />
      )}
    </div>
  );
};

export default TestCaseEventListener;
