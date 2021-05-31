import React, { useEffect, useState } from "react";
import { cFetch } from "../common/util/cFetch";
import { HashScroll } from "../sharedComponents/HashScroll";

interface RelatedPage {
    pageName: string;
    relatedWord: string;
    link: string;
    explanation: string;
}

interface Props {
    location: Location;
    match: { params: { word: string } };
}

export default function Page({
    location,
    match: {
        params: { word },
    },
}: Props) {
    const [pages, setPages] = useState<RelatedPage[]>([]);

    useEffect(() => {
        const load = async () => {
            setPages(await fetchRelatedPages(word));
        };
        void load();
    }, []);

    return (
        <div>
            {word}
            {pages.map(p => p.pageName)}
            <HashScroll location={location} allLoadFinished={true} />
        </div>
    );
}

async function fetchRelatedPages(word: string): Promise<RelatedPage[]> {
    return (await cFetch(`api/PagesAboutJapan/GetPageData/${word}`)).json();
}
