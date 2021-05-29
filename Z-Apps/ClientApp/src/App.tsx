import * as React from "react";
import { lazy, Suspense } from "react";
import { Route, Switch } from "react-router";
import ScrollMemory from "react-router-scroll-memory";
import * as commonFncs from "./common/functions";
import FooterAnimation from "./JapaneseDictionary/parts/Animations/FooterAnimation";
import ShurikenProgress from "./JapaneseDictionary/parts/Animations/ShurikenProgress";
import Layout from "./JapaneseDictionary/parts/Layout";
import { PopupAd } from "./JapaneseDictionary/parts/YouTubeAd/Popup";
import { APP_VERSION } from "./version";

const Dictionary = lazy(() => import("./JapaneseDictionary/Dictionary"));
const DictionaryTop = lazy(() => import("./JapaneseDictionary/Dictionary/Top"));
const DictionaryEdit = lazy(
    () => import("./JapaneseDictionary/Dictionary/Edit")
);
const DictionaryExclude = lazy(
    () => import("./JapaneseDictionary/Dictionary/Exclude")
);
const NotFound = lazy(() => import("./JapaneseDictionary/404"));

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
                        <Route
                            sensitive
                            exact
                            path="/dictionary/:word"
                            component={Dictionary}
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
