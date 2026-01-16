import React, { useEffect, useReducer, useState, useCallback, memo } from "react";
import "./IssueContextOverview.css";
import { VscCheck, VscError } from "react-icons/vsc";
import { AiOutlineLoading3Quarters } from "react-icons/ai";

type ItemStatus = "loading" | "success" | "error";
type ItemType = "confluence" | "subtask";

interface ItemState {
  id: string;
  title: string;
  status: ItemStatus;
  errorMessage?: string;
}

type ItemsState = Record<string, ItemState>;

// Normalized event type for reducer
type ItemEvent =
  | { type: "start"; id: string; title: string; itemType: ItemType }
  | { type: "finished"; id: string; itemType: ItemType }
  | { type: "error"; id: string; message: string; itemType: ItemType };

// Reducer to handle normalized ItemEvents
const itemsReducer = (state: ItemsState, event: ItemEvent): ItemsState => {
  const current = state[event.id];

  switch (event.type) {
    case "start":
      return {
        ...state,
        [event.id]: { id: event.id, title: event.title, status: "loading" },
      };

    case "finished":
      if (!current || current.status === "error") return state;
      return { ...state, [event.id]: { ...current, status: "success" } };

    case "error":
      return {
        ...state,
        [event.id]: { ...current, status: "error", errorMessage: event.message },
      };

    default:
      return state;
  }
};

// Helper to normalize SSE events into ItemEvents
const normalizeEvent = (eventName: string, data: any): ItemEvent | null => {
  const confluencePrefix = "confluence-";
  const subtaskPrefix = "subtask-";

  switch (eventName) {
    case "confluencePageStart":
      return { type: "start", id: confluencePrefix + data.pageId, title: data.title, itemType: "confluence" };
    case "confluencePageFinished":
      return { type: "finished", id: confluencePrefix + data.pageId, itemType: "confluence" };
    case "confluencePageError":
      return { type: "error", id: confluencePrefix + data.pageId, message: data.error, itemType: "confluence" };

    case "subtaskStart":
      return { type: "start", id: subtaskPrefix + data.subtaskId, title: data.title, itemType: "subtask" };
    case "subtaskFinished":
      return { type: "finished", id: subtaskPrefix + data.subtaskId, itemType: "subtask" };
    case "subtaskError":
      return { type: "error", id: subtaskPrefix + data.subtaskId, message: data.error, itemType: "subtask" };

    default:
      return null;
  }
};

// ItemRow
const ItemRow: React.FC<{ item: ItemState }> = memo(({ item }) => {
  const renderStatusIcon = () => {
    switch (item.status) {
      case "loading":
        return <AiOutlineLoading3Quarters className="icon spinner" />;
      case "success":
        return <VscCheck className="icon success" />;
      case "error":
        return (
          <div className="tooltipContainer">
            <VscError className="icon error" />
            {item.errorMessage && <div className="tooltipBox errorBox">{item.errorMessage}</div>}
          </div>
        );
      default:
        return null;
    }
  };

  const href =
    item.id.startsWith("confluence-")
      ? `https://dayforce.atlassian.net/wiki/pages/viewpage.action?pageId=${item.id.replace("confluence-", "")}`
      : item.id.startsWith("subtask-")
      ? `https://dayforce.atlassian.net/browse/${item.id.replace("subtask-", "")}`
      : undefined;

  const itemContent = <span className="itemContent">- {item.title}</span>;

  return (
    <div className="listItem">
      {href ? (
        <a href={href} target="_blank" rel="noopener noreferrer" className="itemLink">
          {itemContent}
        </a>
      ) : (
        itemContent
      )}
      <div className="iconsRight">{renderStatusIcon()}</div>
    </div>
  );
});

interface IssueContextOverviewProps {
    eventSource: EventSource | null;
}


const IssueContextOverview: React.FC<IssueContextOverviewProps> = ({ eventSource }) => {
  const [jiraKey, setJiraKey] = useState("");
  const [jiraTitle, setJiraTitle] = useState("");
  const [items, dispatch] = useReducer(itemsReducer, {});
  const [testCaseStarted, setTestCaseStarted] = useState(false);
  const [subtaskStarted, setSubtaskStarted] = useState(false);

  const groupedItems = Object.values(items).reduce(
    (acc: Record<"confluence" | "subtask", ItemState[]>, item) => {
      if (item.id.startsWith("confluence-")) acc.confluence.push(item);
      else if (item.id.startsWith("subtask-")) acc.subtask.push(item);
      return acc;
    },
    { confluence: [], subtask: [] }
  );

  const addSSEListener = useCallback(
  (eventName: string) => {
    eventSource.addEventListener(eventName, (e: MessageEvent<string>) => {
      const data = JSON.parse(e.data);

      if (eventName === "jiraFetched") {
        if (data.jiraKey) setJiraKey(data.jiraKey);
        if (data.jiraTitle) setJiraTitle(data.jiraTitle);
      } else if (eventName === "testCaseGenerated") {
        setTestCaseStarted(true);
      } else if (eventName === "subtaskStart") {
        setSubtaskStarted(true);
        const event = normalizeEvent(eventName, data);
        if (event) dispatch(event);
      } else {
        const event = normalizeEvent(eventName, data);
        if (event) dispatch(event);
      }
    });
  },
  [eventSource]
);


  useEffect(() => {
    if (!eventSource) return;

    [
      "jiraFetched",
      "confluencePageStart",
      "confluencePageFinished",
      "confluencePageError",
      "subtaskStart",
      "subtaskFinished",
      "subtaskError",
      "testCaseGenerated",
    ].forEach(addSSEListener);

    return () => eventSource.close();
  }, [eventSource, addSSEListener]);

  const renderSkeletons = (count: number) => Array.from({ length: count }).map((_, idx) => <div key={idx} className="skeletonItem" />);

  return (
    <div className="issueContextContainer">
      <div className="titleWrapper">
        {jiraKey && jiraTitle ? (
          <a
            className="title"
            target="_blank"
            rel="noopener noreferrer"
            href={`https://dayforce.atlassian.net/browse/${jiraKey}`}
          >
            {jiraKey}: {jiraTitle}
          </a>
        ) : (
          <span className="skeletonTitle"></span>
        )}
      </div>

      <div>
        {!testCaseStarted && !subtaskStarted && groupedItems.confluence.length === 0 ? (
          <>
            <div className="skeletonSubtitle"></div>
            {renderSkeletons(4)}
          </>
        ) : groupedItems.confluence.length > 0 ? (
          <>
            <p className="subtitle">Relevant Confluence pages:</p>
            {groupedItems.confluence.map((item) => (
              <ItemRow key={item.id} item={item} />
            ))}
          </>
        ) : null}
      </div>

      <div>
        {!testCaseStarted && groupedItems.subtask.length === 0 ? (
          <>
            <div className="skeletonSubtitle"></div>
            {renderSkeletons(1)}
          </>
        ) : groupedItems.subtask.length > 0 ? (
          <>
            <p className="subtitle">Relevant Subtask:</p>
            {groupedItems.subtask.map((item) => (
              <ItemRow key={item.id} item={item} />
            ))}
          </>
        ) : null}
      </div>
    </div>
  );
};

export default IssueContextOverview;
