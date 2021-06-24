import * as React from "react";
import { lazy, Suspense } from "react";
import { Route, Switch } from "react-router";
import ScrollMemory from "react-router-scroll-memory";
import { reloadAndRedirect_OneTimeReload } from "../common/functions";
import { ReturnToLocalMenu } from "../LocalDebug/LocalDebugMenu";
import ShurikenProgress from "../sharedComponents/Animations/ShurikenProgress";
import { Helmet } from "../sharedComponents/Helmet";
import { APP_VERSION } from "../version";

const NotFound = lazy(() => import("../sharedComponents/404"));

const AdminMenu = lazy(() => import("./AdminMenu"));
const WikiLog = lazy(() => import("./WikiLog"));
const SitemapCount = lazy(() => import("./SitemapCount"));
const ApiCache = lazy(() => import("./ApiCache"));

export function App() {
    return (
        <div style={{ margin: 30 }}>
            <Helmet noindex />
            <Suspense fallback={<LoadingAnimation num={1} />}>
                <ScrollMemory />
                <Switch>
                    <Route sensitive exact path="/" component={AdminMenu} />
                    <Route
                        sensitive
                        exact
                        path="/wiki-log"
                        component={WikiLog}
                    />
                    <Route
                        sensitive
                        exact
                        path="/sitemap-count"
                        component={SitemapCount}
                    />
                    <Route
                        sensitive
                        exact
                        path="/api-cache"
                        component={ApiCache}
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
        </div>
    );
}

function NotFoundRedirect() {
    const redirect = async () => {
        const url = `api/SystemBase/GetVersion/V${new Date().getMilliseconds()}`;
        const res = await fetch(url);
        const v = await res.text();

        if (Number(v) !== APP_VERSION) {
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
