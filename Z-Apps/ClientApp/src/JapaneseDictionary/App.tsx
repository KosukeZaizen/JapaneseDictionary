import * as React from "react";
import { lazy, Suspense } from "react";
import { Route, Switch } from "react-router";
import ScrollMemory from "react-router-scroll-memory";
import { reloadAndRedirect_OneTimeReload } from "../common/functions";
import { ReturnToLocalMenu } from "../LocalDebug/LocalDebugMenu";
import FooterAnimation from "../sharedComponents/Animations/FooterAnimation";
import ShurikenProgress from "../sharedComponents/Animations/ShurikenProgress";
import { PopupAd } from "../sharedComponents/YouTubeAd/Popup";
import { APP_VERSION } from "../version";
import "./css/index.css";
import Layout from "./Layout/Layout";

const Dictionary = lazy(() => import("./Dictionary"));
const DictionaryTop = lazy(() => import("./Dictionary/Top"));
const DictionaryEdit = lazy(() => import("./Dictionary/Edit"));
const DictionaryExclude = lazy(() => import("./Dictionary/Exclude"));
const NotFound = lazy(() => import("../sharedComponents/404"));

export function App() {
    return (
        <Layout>
            <Suspense fallback={<LoadingAnimation num={1} />}>
                <ScrollMemory />
                <Switch>
                    <Route sensitive exact path="/" component={DictionaryTop} />
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
                        exact
                        path="/local"
                        component={ReturnToLocalMenu}
                    />
                    <Route sensitive path="/not-found" component={NotFound} />
                    <Route component={NotFoundRedirect} />
                </Switch>
            </Suspense>
            <FooterAnimation />
            <PopupAd />
        </Layout>
    );
}

function NotFoundRedirect() {
    const redirect = async () => {
        const url = `api/SystemBase/GetVersion/V${new Date().getMilliseconds()}`;
        const res = await fetch(url);
        const v = await res.text();

        if (Number(v) !== APP_VERSION) {
            // @ts-ignore
            window.location.reload(true);
        } else {
            reloadAndRedirect_OneTimeReload("pageNotFoundRedirect");
        }
    };
    void redirect();

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
