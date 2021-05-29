import * as React from "react";
import { Link } from "react-router-dom";
import Button from "reactstrap/lib/Button";
import Card from "reactstrap/lib/Card";
import CardText from "reactstrap/lib/CardText";
import CardTitle from "reactstrap/lib/CardTitle";
import { cFetch } from "../../common/util/cFetch";
import ShurikenProgress from "../../sharedComponents/Animations/ShurikenProgress";
import FB from "../../sharedComponents/FaceBook";
import Head from "../../sharedComponents/Helmet";

class DictionaryTop extends React.Component<
    {},
    {
        words: string[];
    }
> {
    ref: React.RefObject<HTMLDivElement>;

    constructor(props: {}) {
        super(props);

        this.state = {
            words: [],
        };

        this.ref = React.createRef();
    }

    componentDidMount() {
        const getData = async () => {
            const url = `api/Wiki/GetAllWords?num=500`;
            const response = cFetch(url);

            const urlAll = `api/Wiki/GetAllWords?num=0`;
            const responseAll = cFetch(urlAll);

            this.setState({
                words: await (await response).json(),
            });

            this.setState({
                words: await (await responseAll).json(),
            });
        };
        getData();
    }

    render() {
        const styleForAboutTitle = {
            background: "#fee8b4",
            boxShadow: "0px 0px 0px 5px #fee8b4",
            border: "dashed 2px white",
            padding: "0.2em 0.5em",
            marginBottom: "10px",
        };
        return (
            <div className="center">
                <Head
                    title="Japanese dictionary"
                    desc="Free website to learn the meaning of Japanese words! You can learn a lot of Japanese words from this page!"
                />
                <div style={{ maxWidth: 700 }}>
                    <h1
                        style={{
                            margin: "30px",
                            lineHeight: "40px",
                        }}
                        className="whiteShadow"
                    >
                        <b>Japanese Dictionary</b>
                    </h1>
                    <p style={styleForAboutTitle}>
                        Free website to learn the meaning of Japanese words!
                        <br />
                        You can learn a lot of Japanese words from this page!
                    </p>
                    <br />
                    {this.state.words.length > 0 ? (
                        this.state.words.map(w => (
                            <div key={w}>
                                <Link
                                    to={"dictionary/" + encodeURIComponent(w)}
                                >
                                    {w}
                                </Link>
                            </div>
                        ))
                    ) : (
                        <ShurikenProgress key="circle" size="20%" />
                    )}
                    <hr />
                    <div style={{ fontSize: "x-large", margin: "20px" }}>
                        <a href="https://www.lingual-ninja.com/folktales">
                            {"Learn Japanese from Japanese folktales >>"}
                        </a>
                    </div>
                    <hr />
                    <a href="https://www.lingual-ninja.com/vocabulary-list">
                        <Card
                            body
                            style={{
                                backgroundColor: "#333",
                                borderColor: "#333",
                                color: "white",
                            }}
                        >
                            <CardTitle>Japanese Vocabulary List</CardTitle>
                            <CardText>
                                Basic Japanese Vocabulary List!
                                <br />
                                Try to memorize all the vocabulary by using the
                                quizzes!
                            </CardText>
                            <Button color="secondary">Try!</Button>
                        </Card>
                    </a>
                    <hr />
                </div>
                <FB />
            </div>
        );
    }
}

export default DictionaryTop;
