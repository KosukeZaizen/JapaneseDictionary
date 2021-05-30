import * as React from "react";
import { useEffect } from "react";

export default function ApiCache() {
    useEffect(() => {
        const getCache = async () => {
            const result = await (
                await fetch("api/SystemBase/GetCache")
            ).json();

            console.log("cache", result);
        };
        getCache();
    }, []);

    return (
        <>
            <h2>ApiCache</h2>
            Check console
        </>
    );
}
