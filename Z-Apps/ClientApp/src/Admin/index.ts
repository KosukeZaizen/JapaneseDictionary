import { AppToMount } from "..";

export const appToMount: AppToMount = {
    key: "Admin",
    hostname: "admin.lingual-ninja.com",
    getApp: async () => {
        const module = await import("./App");
        return module.App;
    },
};
