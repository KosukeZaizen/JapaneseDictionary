import React from "react";
import { Link } from "react-router-dom";

type Pages = {
    url: string;
    title: string;
}[];

const pages: Pages = [
    {
        url: "/wiki-log",
        title: "Wiki Log",
    },
    {
        url: "/sitemap-count",
        title: "Sitemap Count",
    },
];

export default function AdminMenu() {
    return (
        <>
            <h2>Admin Menu</h2>
            <ul style={{ marginTop: 30 }}>
                {pages.map(p => (
                    <li key={p.url} style={{ margin: 10 }}>
                        <Link to={p.url}>{p.title}</Link>
                    </li>
                ))}
            </ul>
        </>
    );
}
