import React, { useEffect, useState } from "react";
import { Link, RouteComponentProps } from "react-router-dom";
import Breadcrumb from "reactstrap/lib/Breadcrumb";
import BreadcrumbItem from "reactstrap/lib/BreadcrumbItem";
import Card from "reactstrap/lib/Card";
import CardText from "reactstrap/lib/CardText";
import CardTitle from "reactstrap/lib/CardTitle";
import { reloadAndRedirect_OneTimeReload } from "../common/functions";
import { cFetch } from "../common/util/cFetch";
import ShurikenProgress from "../sharedComponents/Animations/ShurikenProgress";
import FB from "../sharedComponents/FaceBook";
import { HashScroll } from "../sharedComponents/HashScroll";
import Helmet from "../sharedComponents/Helmet";
import { YouTubeAd } from "../sharedComponents/YouTubeAd";

interface RelatedPage {
    pageName: string;
    relatedWord: string;
    link: string;
    explanation: string;
}

interface Category {
    category: string;
    words: string[];
}

const initialCategory: Category = { category: "", words: [] };

type Props = RouteComponentProps<{ word: string }>;

export default function Page({
    location,
    match: {
        params: { word: originalWord },
    },
}: Props) {
    const [pages, setPages] = useState<RelatedPage[]>([]);
    const [category, setCategory] = useState(initialCategory);

    useEffect(() => {
        setPages([]);
        setCategory(initialCategory);

        const load = async () => {
            setPages(await fetchRelatedPages(originalWord));
            setCategory(await fetchSameCategoryWords(originalWord));
        };
        void load();
    }, [originalWord]);

    const word = originalWord.split("%3A").join(":");

    const desc = `Pages about ${word}.
Visit the pages below to learn about ${word}.`;

    return (
        <>
            <Helmet title={word} desc={desc} />

            <Breadcrumb>
                <BreadcrumbItem>
                    <Link to="/">Home</Link>
                </BreadcrumbItem>
                <BreadcrumbItem active>{word}</BreadcrumbItem>
            </Breadcrumb>

            <h1 style={{ marginTop: 20 }}>{word}</h1>

            <p style={{ whiteSpace: "pre-wrap", margin: "20px 0 30px" }}>
                {desc}
            </p>

            {pages.map(p => (
                <Card body style={{ marginTop: 20 }} key={p.link}>
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
            {category.words.length > 1 && (
                <Card body style={{ marginTop: 20 }}>
                    <CardTitle tag="h2" style={{ marginBottom: 15 }}>
                        {category.category}
                    </CardTitle>
                    <CardText>
                        {category.words.map((w, i) => (
                            <span key={w}>
                                {i !== 0 && " / "}
                                {w === word ? (
                                    w
                                ) : (
                                    <Link to={`/p/${encodeURIComponent(w)}`}>
                                        {w}
                                    </Link>
                                )}
                            </span>
                        ))}
                    </CardText>
                </Card>
            )}
            {pages.length <= 0 && <ShurikenProgress size="20%" />}
            <aside
                style={{
                    maxWidth: 400,
                    marginTop: 15,
                    marginBottom: 15,
                }}
            >
                <YouTubeAd />
            </aside>
            <FB style={{ display: "inline" }} />
            <HashScroll location={location} allLoadFinished={true} />
        </>
    );
}

async function fetchRelatedPages(word: string) {
    try {
        const res: RelatedPage[] = await (
            await cFetch(`api/PagesAboutJapan/GetPageData/${word}`)
        ).json();

        if (res?.length > 0) {
            return res;
        }
    } catch (ex) {}
    reloadAndRedirect_OneTimeReload("pagesAboutJapan-relatedPages");
    return [];
}

async function fetchSameCategoryWords(word: string) {
    try {
        const res: Category = await (
            await cFetch(`api/PagesAboutJapan/GetSameCategoryWords/${word}`)
        ).json();

        if (res?.category && Array.isArray(res?.words)) {
            return res;
        }
    } catch (ex) {}
    return initialCategory;
}
