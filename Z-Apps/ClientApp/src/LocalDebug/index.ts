import { AppToMount } from "..";

export const appToMount: AppToMount = {
    key: "LocalDebugMenu",
    hostname: "localhost",
    getApp: async () => {
        const module = await import("./LocalDebugMenu");
        return module.LocalDebugMenu;
    },
};
