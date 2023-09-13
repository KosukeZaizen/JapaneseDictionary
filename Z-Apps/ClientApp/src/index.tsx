import "bootstrap/dist/css/bootstrap.css";
import { createBrowserHistory } from "history";
import * as React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { ConnectedRouter } from "react-router-redux";
import { appToMount as Admin } from "./Admin";
import { appToMount as JapaneseDictionary } from "./JapaneseDictionary";
import { appToMount as LocalDebugMenu } from "./LocalDebug";
import { startAnimation } from "./common/animation";
import { azureUrl, siteUrl } from "./common/consts";
import { checkAppVersion } from "./common/functions";
//import registerServiceWorker from './registerServiceWorker';
import { unregister } from "./registerServiceWorker";
import configureStore from "./store/configureStore";

//AzureUrlから通常のURLへリダイレクト
if (window.location.href.includes(azureUrl)) {
    window.location.href = window.location.href.replace(azureUrl, siteUrl);
}

checkAppVersion();

// Create browser history to use in the Redux store
const baseUrl =
    document.getElementsByTagName("base")[0].getAttribute("href") ?? undefined;
const history = createBrowserHistory({ basename: baseUrl });

startAnimation();

// Get the application-wide store instance, prepopulating with state from the server where available.
const store = configureStore(history);

const rootElement = document.getElementById("root");

export interface AppToMount {
    key: string;
    hostname: string;
    getApp: () => Promise<React.FunctionComponent>;
}

// アプリ追加時は、この配列に追加
export const apps: AppToMount[] = [
    JapaneseDictionary,
    // PagesAboutJapan,
    Admin,
    LocalDebugMenu,
];
const appObject = apps.find(a => window.location.hostname.includes(a.hostname));

if (appObject?.key === "LocalDebugMenu") {
    const savedAppKey = window.localStorage.getItem("appKeyToMount");
    const savedApp = apps.find(a => a.key === savedAppKey);
    if (savedApp) {
        appObject.getApp = savedApp.getApp;
    }
}

if (!appObject) {
    window.location.href = "https://dictionary.lingual-ninja.com";
} else {
    const render = async () => {
        const App = await appObject.getApp();
        ReactDOM.render(
            <Provider store={store}>
                <ConnectedRouter history={history}>
                    <App />
                </ConnectedRouter>
            </Provider>,
            rootElement
        );
    };
    render();
}

//registerServiceWorker();
unregister();
