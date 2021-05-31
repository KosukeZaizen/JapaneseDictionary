import React, { useEffect, useState } from "react";
import Card from "reactstrap/lib/Card";
import CardText from "reactstrap/lib/CardText";
import CardTitle from "reactstrap/lib/CardTitle";
import { reloadAndRedirect_OneTimeReload } from "../common/functions";
import { cFetch } from "../common/util/cFetch";
import ShurikenProgress from "../sharedComponents/Animations/ShurikenProgress";
import { HashScroll } from "../sharedComponents/HashScroll";
import Helmet from "../sharedComponents/Helmet";

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

    const desc = `Pages about ${word}.
Visit the pages below to learn about ${word}.`;

    return (
        <>
            <Helmet title={word} desc={desc} />

            <h1>{word}</h1>

            <p style={{ whiteSpace: "pre-wrap", margin: "20px 0 30px" }}>
                {desc}
            </p>

            {pages.map(p => (
                <Card body style={{ marginTop: 20 }}>
                    <CardTitle tag="h2" style={{ marginBottom: 15 }}>
                        {p.pageName}
                    </CardTitle>
                    <CardText>
                        {p.explanation}{" "}
                        <a
                            href={p.link}
                            target="_blank"
                            rel="noopener noreferrer nofollow"
                        >
                            {"Visit >>"}
                        </a>
                    </CardText>
                </Card>
            ))}
            {pages.length <= 0 && <ShurikenProgress size="20%" />}
            <HashScroll location={location} allLoadFinished={true} />
        </>
    );
}

async function fetchRelatedPages(word: string): Promise<RelatedPage[]> {
    try {
        const res = await (
            await cFetch(`api/PagesAboutJapan/GetPageData/${word}`)
        ).json();

        if (res?.length > 0) {
            return res;
        }
    } catch (ex) {}
    reloadAndRedirect_OneTimeReload("pagesAboutJapan-relatedPages");
    return [];
}
