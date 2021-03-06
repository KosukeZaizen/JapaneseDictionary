import React from "react";
import { BLOB_URL } from "../../../common/consts";
import { ScrollBox } from "../../../sharedComponents/ScrollBox";
import { storyDesc } from "../types/stories";

const HeadTitle = ({
    children,
    headLevel,
}: {
    children: React.ReactNode;
    headLevel: "h2" | "h3";
}) =>
    headLevel === "h3" ? (
        <h3
            style={{
                marginBottom: "20px",
                fontWeight: "bolder",
            }}
        >
            {children}
        </h3>
    ) : (
        <h2
            style={{
                marginBottom: "20px",
                fontWeight: "bolder",
            }}
        >
            {children}
        </h2>
    );

export function StoriesList({
    stories,
    screenWidth,
    headLevel,
}: {
    stories: storyDesc[];
    screenWidth: number;
    headLevel: "h2" | "h3";
}) {
    const isWide = screenWidth > 500;

    return (
        <>
            {stories?.map(s => {
                const nameForUrl = s.storyName;
                const nameToShow = s.storyName
                    .split("--")
                    .join(" - ")
                    .split("_")
                    .join(" ");

                return (
                    <section
                        key={s.storyId}
                        style={{
                            marginTop: 45,
                        }}
                    >
                        <ScrollBox>
                            <HeadTitle headLevel={headLevel}>
                                <a
                                    href={`https://www.lingual-ninja.com/folktales/${nameForUrl}`}
                                    style={{
                                        color: "black",
                                    }}
                                >
                                    {nameToShow}
                                </a>
                            </HeadTitle>
                            <div
                                style={{
                                    display: "flex",
                                    flexDirection: isWide ? "row" : "column",
                                    fontSize: isWide ? "large" : "medium",
                                    alignItems: "center",
                                }}
                            >
                                <a
                                    href={`https://www.lingual-ninja.com/folktales/${nameForUrl}`}
                                    style={{
                                        flex: 1,
                                        margin: isWide
                                            ? undefined
                                            : "10px 0 15px",
                                    }}
                                >
                                    <img
                                        src={`${BLOB_URL}/folktalesImg/${
                                            nameForUrl.split("--")[0]
                                        }.png`}
                                        width={isWide ? "90%" : "100%"}
                                        alt={nameToShow}
                                        title={nameToShow}
                                    />
                                </a>
                                <div
                                    style={{
                                        flex: 1,
                                        display: "flex",
                                        justifyContent: "center",
                                        flexDirection: "column",
                                    }}
                                >
                                    <div
                                        style={{
                                            textAlign: "left",
                                            margin: isWide ? "10px" : "10px 0",
                                        }}
                                    >
                                        {s.description
                                            .split("\\n")
                                            .map((d, i) => (
                                                <span
                                                    key={i}
                                                    style={{
                                                        color: "black",
                                                    }}
                                                >
                                                    {d}
                                                    <br />
                                                </span>
                                            ))}
                                    </div>
                                    <p>
                                        <a
                                            href={`https://www.lingual-ninja.com/folktales/${nameForUrl}`}
                                        >{`Read ${nameToShow} >>`}</a>
                                    </p>
                                </div>
                            </div>
                        </ScrollBox>
                    </section>
                );
            })}
        </>
    );
}
