import * as React from "react";
import { useEffect } from "react";
import { RouteComponentProps } from "react-router-dom";
import Head from "../../sharedComponents/Helmet";

type Props = RouteComponentProps<{ word: string }>;
export default function Exclude(props: Props) {
    useEffect(() => {
        const {
            match: {
                params: { word },
            },
        } = props;

        if (!window.confirm(`Do you really want to exclude "${word}"?`)) {
            return;
        }

        const saveData = localStorage.getItem("folktales-register-token");
        const objSaveData = saveData && JSON.parse(saveData);
        const token = objSaveData?.token || "";

        const formData = new FormData();
        formData.append("word", word);
        formData.append("token", token);

        fetch("/api/JapaneseDictionary/Exclude", {
            method: "POST",
            body: formData,
        });
    }, []);

    return (
        <div>
            <Head noindex />
            exclude screen
        </div>
    );
}
