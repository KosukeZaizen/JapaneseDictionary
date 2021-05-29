import React from "react";
import { AppToMount } from "..";

export const appToMount: AppToMount = {
    key: "LocalDebugMenu",
    hostname: "localhost",
    app: LocalDebugMenu,
};

function LocalDebugMenu() {
    return (
        <div>
            Local Debug Menu
            <br />
            <button
                onClick={() => {
                    saveKey("JapaneseDictionary");
                }}
            >
                Japanese Dictionary
            </button>
        </div>
    );
}

export function ReturnToLocalMenu() {
    if (window.location.hostname === "localhost") {
        localStorage.removeItem("appKeyToMount");
        window.location.href = "/";
    } else {
        window.location.href = `/not-found?p=${window.location.pathname}`;
    }
    return null;
}

function saveKey(appKey: string) {
    window.localStorage.setItem("appKeyToMount", appKey);
    window.location.reload();
}
