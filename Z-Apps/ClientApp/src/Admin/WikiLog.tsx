import React, { useEffect, useState } from "react";

interface WikiLog {
    procType: string;
    term: string;
    startTime: string;
    endTime: string;
    message: string | null;
}

const tdStyle = { border: "solid", padding: 5 };
const thStyle = { border: "solid", padding: 5, backgroundColor: "ivory" };

export default function WikiLog() {
    const [logs, setLogs] = useState<WikiLog[]>([]);

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
                            <td style={tdStyle}>{log.message}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
}

async function fetchWikiLog(): Promise<WikiLog[]> {
    const res = await fetch("api/Admin/GetWikiLog");
    return await res.json();
}
