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
import Layout from "./Layout/Layout";

const Top = lazy(() => import("./Top"));
const Page = lazy(() => import("./Page"));
const NotFound = lazy(() => import("../sharedComponents/404"));

export function App() {
    return (
        <Layout>
            <Suspense fallback={<LoadingAnimation num={1} />}>
                <ScrollMemory />
                <Switch>
                    <Route sensitive exact path="/" component={Top} />
                    <Route sensitive exact path="/p/:word" component={Page} />
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
