import React, { useEffect, useState } from "react";

interface SitemapCount {
    lang: "ja" | "en";
    time: string;
    category: number;
    word: number;
    cached: number;
}

export default function SitemapCount() {
    const [sitemapCount, setSitemapCount] = useState<SitemapCount[]>([]);

    useEffect(() => {
        const load = async () => {
            setSitemapCount(await fetchSitemapCount());
        };
        void load();
    }, []);

    console.log("window.innerWidth", window.innerWidth);

    return (
        <>
            <h2>Sitemap Count</h2>
            {sitemapCount.map(s => (
                <div
                    key={s.lang + s.time}
                    style={{
                        border: "solid",
                        margin: 10,
                        padding: 20,
                        backgroundColor:
                            s.lang === "ja" ? "blanchedalmond" : "lightcyan",
                        width: window.innerWidth > 600 ? "40%" : undefined,
                        display: "inline-block",
                    }}
                >
                    <h3>{s.lang}</h3>
                    <p>{s.time.substr(0, 16)}</p>
                    <p>
                        {"Category Count: "}
                        {s.category}
                    </p>
                    <p>
                        {"Word Count: "}
                        {s.word}
                    </p>
                    <p>
                        {"Cached Words: "}
                        {s.cached}
                    </p>
                </div>
            ))}
        </>
    );
}

async function fetchSitemapCount(): Promise<SitemapCount[]> {
    const res = await fetch("api/Admin/GetSitemapCount");
    return await res.json();
}
