import { AppToMount } from "..";

export const appToMount: AppToMount = {
    key: "JapaneseDictionary",
    hostname: "dictionary.lingual-ninja.com",
    getApp: async () => {
        const module = await import("./App");
        return module.App;
    },
};
