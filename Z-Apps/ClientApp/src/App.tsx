import * as React from "react";
import { lazy, Suspense } from "react";
import { Route, Switch } from "react-router";
import ScrollMemory from "react-router-scroll-memory";
import * as commonFncs from "./common/functions";
import FooterAnimation from "./components/parts/Animations/FooterAnimation";
import ShurikenProgress from "./components/parts/Animations/ShurikenProgress";
import Layout from "./components/parts/Layout";
import { PopupAd } from "./components/parts/YouTubeAd/Popup";
import { APP_VERSION } from "./version";

const Home = lazy(() => import("./components/Home"));
const Terms = lazy(() => import("./components/Terms"));
const Developer = lazy(() => import("./components/Developer"));
const RomajiConverter = lazy(() => import("./components/RomajiConverter"));
const KanjiConverter = lazy(() => import("./components/KanjiConverter"));
const HiraganaAndKatakana = lazy(
    () => import("./components/HiraganaAndKatakana")
);
const HiraganaQuiz = lazy(() => import("./components/HiraganaQuiz"));
const KatakanaQuiz = lazy(() => import("./components/KatakanaQuiz"));
const VocabList = lazy(() => import("./components/Vocab/VocabList"));
const VocabQuiz = lazy(() => import("./components/Vocab/VocabQuiz"));
const VocabQuizTop = lazy(() => import("./components/Vocab/VocabQuizTop"));
const VocabKanjiQuiz = lazy(() => import("./components/Vocab/VocabKanjiQuiz"));
const VocabKanjiQuizTop = lazy(
    () => import("./components/Vocab/VocabKanjiQuizTop")
);
const VocabVideo = lazy(() => import("./components/Vocab/VocabVideo"));
const VocabEdit = lazy(() => import("./components/Vocab/Edit"));
const VocabEditTop = lazy(() => import("./components/Vocab/Edit/Top"));
const Stories = lazy(() => import("./components/Stories"));
const StoriesTop = lazy(() => import("./components/Stories/StoriesTop"));
const StoriesEdit = lazy(() => import("./components/Stories/StoriesEdit"));
const StoriesEditTop = lazy(
    () => import("./components/Stories/StoriesTop/StoriesEditTop")
);
const StoriesVideo = lazy(() => import("./components/Stories/StoriesVideo"));
const NinjaTop = lazy(() => import("./components/Games/NinjaGameTop"));
const Ninja1 = lazy(() => import("./components/Games/NinjaGame"));
const Ninja2 = lazy(() => import("./components/Games/NinjaGame2"));
const Ninja3 = lazy(() => import("./components/Games/NinjaGame3"));
const GameToLearn = lazy(() => import("./components/Games/GameToLearn"));
const GameOver = lazy(() => import("./components/Games/GameOver"));
const Dictionary = lazy(() => import("./components/Dictionary"));
const DictionaryTop = lazy(() => import("./components/Dictionary/Top"));
const DictionaryEdit = lazy(() => import("./components/Dictionary/Edit"));
const DictionaryExclude = lazy(() => import("./components/Dictionary/Exclude"));
const Articles = lazy(() => import("./components/Articles"));
const ArticlesTop = lazy(() => import("./components/Articles/Top"));
const ArticlesEditTop = lazy(() => import("./components/Articles/EditTop"));
const ArticlesEdit = lazy(() => import("./components/Articles/Edit"));
const SiteMapEdit = lazy(() => import("./components/SiteMapEdit"));
const Admin = lazy(() => import("./components/Admin"));
const ApiCache = lazy(() => import("./components/Admin/ApiCache"));
const OpeLogTable = lazy(() => import("./components/Admin/OpeLogTable"));
const ColorPalette = lazy(() => import("./components/ColorPalette"));
const Boscobel = lazy(() => import("./components/Boscobel"));
const NotFound = lazy(() => import("./components/404"));

export default class App extends React.Component {
    render() {
        return (
            <Layout>
                <Suspense fallback={<LoadingAnimation num={1} />}>
                    <ScrollMemory />
                    <Switch>
                        <Route
                            sensitive
                            exact
                            path="/"
                            component={DictionaryTop}
                        />
                        {/* <Route
                            sensitive
                            exact
                            path="/dictionary"
                            component={DictionaryTop}
                        /> */}
                        <Route
                            sensitive
                            exact
                            path="/dictionary/:word"
                            component={Dictionary}
                        />
                        <Route
                            sensitive
                            exact
                            path="/how-to-read-japanese/:word"
                            component={(props: {
                                match: { params: { word: string } };
                            }) => {
                                const {
                                    match: {
                                        params: { word },
                                    },
                                } = props;
                                window.location.href = `/dictionary/${word}`;
                                return null;
                            }}
                        />
                        <Route
                            sensitive
                            exact
                            path="/dictionaryEdit/:word"
                            component={DictionaryEdit}
                        />
                        <Route
                            sensitive
                            exact
                            path="/dictionaryExclude/:word"
                            component={DictionaryExclude}
                        />
                        <Route sensitive path="/admin" component={Admin} />
                        <Route
                            sensitive
                            path="/apiCache"
                            component={ApiCache}
                        />
                        <Route
                            sensitive
                            path="/not-found"
                            component={NotFound}
                        />
                        <Route component={NotFoundRedirect} />
                    </Switch>
                </Suspense>
                <FooterAnimation />
                <PopupAd />
            </Layout>
        );
    }
}

function NotFoundRedirect() {
    const url = `api/SystemBase/GetVersion/V${new Date().getMilliseconds()}`;
    fetch(url).then(res => {
        res.json().then(v => {
            if (Number(v) !== APP_VERSION) {
                window.location.reload(true);
            } else {
                commonFncs.reloadAndRedirect_OneTimeReload(
                    "pageNotFoundRedirect"
                );
            }
        });
    });

    return (
        <div>
            <LoadingAnimation num={1} />
        </div>
    );
}

export function LoadingAnimation(props: { num: number }) {
    let arr = [];
    for (let i = 0; i < props.num; i++) {
        arr.push(
            <span key={i}>
                <br />
            </span>
        );
    }
    arr.push(
        <ShurikenProgress key="circle" size="20%" style={{ margin: 30 }} />
    );
    return <div className="center">{arr}</div>;
}
