import React, { useEffect, useState } from "react";
import { Collapse } from "reactstrap";

interface WikiLog {
    procType: string;
    term: string;
    startTime: string;
    endTime: string;
    message: string | null;
}

const tdStyle = { border: "solid", padding: 5 };
const thStyle = { border: "solid", padding: 5, backgroundColor: "ivory" };

type NoIndexLogMessage = {
    procType: "zApps_dictionary_noIndex";
    logs: { [key: string]: { length: number; words: string[] } };
};

type LogMessage = NoIndexLogMessage | null;

export default function WikiLog() {
    const [logs, setLogs] = useState<WikiLog[]>([]);
    const [message, setMessage] = useState<LogMessage>(null);

    useEffect(() => {
        const load = async () => {
            setLogs(await fetchWikiLog());
        };
        void load();
    }, []);

    return (
        <>
            <h2>Wiki Log</h2>
            <table
                style={{
                    marginTop: 20,
                    fontSize: "large",
                    whiteSpace: "nowrap",
                }}
            >
                <thead>
                    <tr>
                        <th style={thStyle}>procType</th>
                        <th style={thStyle}>term</th>
                        <th style={thStyle}>startTime</th>
                        <th style={thStyle}>endTime</th>
                        <th style={thStyle}>message</th>
                    </tr>
                </thead>
                <tbody>
                    {logs.map(log => (
                        <tr key={log.procType}>
                            <td style={tdStyle}>{log.procType}</td>
                            <td style={tdStyle}>
                                {log.term.split(" - ").map((t, i) => (
                                    <span key={i}>
                                        {i ? <br /> : null} {t}
                                    </span>
                                ))}
                            </td>
                            <td style={tdStyle}>{log.startTime}</td>
                            <td style={tdStyle}>{log.endTime}</td>
                            <td style={tdStyle}>
                                {log.message && (
                                    <button
                                        onClick={() => {
                                            if (
                                                typeof log.message !== "string"
                                            ) {
                                                return;
                                            }

                                            switch (log.procType) {
                                                case "zApps_dictionary_noIndex":
                                                    setMessage({
                                                        procType: log.procType,
                                                        logs: JSON.parse(
                                                            log.message
                                                        ),
                                                    });
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }}
                                    >
                                        show
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <Message message={message} />
        </>
    );
}

function Message({ message }: { message: LogMessage }) {
    if (!message) {
        return null;
    }

    switch (message.procType) {
        case "zApps_dictionary_noIndex":
            return <NoIndexMessage message={message} />;
        default:
            return null;
    }
}

function NoIndexMessage({ message }: { message: NoIndexLogMessage }) {
    return (
        <>
            {Object.keys(message.logs).map(key => (
                <WordsInfo k={key} key={key} logs={message.logs} />
            ))}
        </>
    );
}

function WordsInfo({
    k,
    logs,
}: {
    k: string;
    logs: NoIndexLogMessage["logs"];
}) {
    const [isOpen, setIsOpen] = useState(false);

    const val = logs[k];
    return (
        <div>
            {k} length: {val.length}
            {val.length > 0 && (
                <button
                    style={{ marginLeft: 30 }}
                    onClick={() => {
                        setIsOpen(!isOpen);
                    }}
                >
                    {isOpen ? "close" : "open"}
                </button>
            )}
            <Collapse isOpen={isOpen}>{val.words.join(", ")}</Collapse>
        </div>
    );
}

async function fetchWikiLog(): Promise<WikiLog[]> {
    const res = await fetch("api/Admin/GetWikiLog");
    return await res.json();
}
