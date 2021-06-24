import { AppToMount } from "..";

export const appToMount: AppToMount = {
    key: "PagesAboutJapan",
    hostname: "japan.lingual-ninja.com",
    getApp: async () => {
        const module = await import("./App");
        return module.App;
    },
};
