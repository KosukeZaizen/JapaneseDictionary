import { useEffect } from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import * as baseStore from "../../JapaneseDictionary/store/BaseStore";

export const HideFooter = connect(null, dispatch =>
    bindActionCreators(baseStore.actionCreators, dispatch)
)((props: baseStore.ActionCreators) => {
    const { hideFooter, showHeaderAndFooter } = props;
    useEffect(() => {
        hideFooter();

        return () => {
            showHeaderAndFooter();
        };
    }, []);
    return null;
});
