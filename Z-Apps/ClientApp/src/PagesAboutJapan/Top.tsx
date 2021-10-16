import React, { useEffect, useState } from "react";
import { Link, RouteComponentProps } from "react-router-dom";
import Breadcrumb from "reactstrap/lib/Breadcrumb";
import BreadcrumbItem from "reactstrap/lib/BreadcrumbItem";
import Card from "reactstrap/lib/Card";
import CardText from "reactstrap/lib/CardText";
import CardTitle from "reactstrap/lib/CardTitle";
import { cFetch } from "../common/util/cFetch";
import ShurikenProgress from "../sharedComponents/Animations/ShurikenProgress";
import FB from "../sharedComponents/FaceBook";
import { HashScroll } from "../sharedComponents/HashScroll";
import Helmet from "../sharedComponents/Helmet";
import { YouTubeAd } from "../sharedComponents/YouTubeAd";

interface Category {
    category: string;
    words: string[];
}

type Props = RouteComponentProps;

const title = "Pages about Japan";
const desc = `Website to introduce articles about Japan.
You can learn about Japan with many topics.`;

export default function Top({ location }: Props) {
    const [categories, setCategories] = useState<Category[]>([]);

    useEffect(() => {
        const load = async () => {
            setCategories(await fetchCategories());
        };
        void load();
    }, []);

    return (
        <article>
            <Helmet title={title} desc={desc} />

            <Breadcrumb>
                <BreadcrumbItem active>Home</BreadcrumbItem>
            </Breadcrumb>

            <h1 style={{ marginTop: 20 }}>{title}</h1>
            <p style={{ whiteSpace: "pre-wrap", marginTop: 20 }}>{desc}</p>
            <div>
                <div
                    style={{
                        border: "5px double #333333",
                        margin: "20px 0",
                        padding: "10px",
                    }}
                >
                    <div
                        style={{
                            fontSize: "x-large",
                            textAlign: "center",
                            marginBottom: 5,
                            fontWeight: "bold",
                        }}
                    >
                        {"Contents"}
                    </div>
                    {categories.length > 0 ? (
                        categories.map((c, i) => {
                            return (
                                <>
                                    <>{i !== 0 && " / "}</>
                                    <Link
                                        to={`/#${encodeURIComponent(
                                            c.category
                                        )}`}
                                        key={c.category}
                                    >
                                        {c.category}
                                    </Link>
                                </>
                            );
                        })
                    ) : (
                        <ShurikenProgress key="circle" size="10%" />
                    )}
                </div>
            </div>
            <div>
                {categories.map(c => (
                    <Card
                        body
                        style={{ marginTop: 20 }}
                        id={encodeURIComponent(c.category)}
                    >
                        <CardTitle tag="h2" style={{ marginBottom: 15 }}>
                            {c.category}
                        </CardTitle>
                        <CardText>
                            {c.words.map((w, i) => (
                                <span key={w}>
                                    {i !== 0 && " / "}
                                    <Link to={`/p/${encodeURIComponent(w)}`}>
                                        {w}
                                    </Link>
                                </span>
                            ))}
                        </CardText>
                    </Card>
                ))}
            </div>
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
            <HashScroll
                location={location}
                allLoadFinished={categories.length > 0}
            />
        </article>
    );
}

async function fetchCategories(): Promise<Category[]> {
    return (await cFetch("api/PagesAboutJapan/GetTopData")).json();
}
