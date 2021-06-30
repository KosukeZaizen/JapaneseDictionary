import { routerMiddleware, routerReducer } from "react-router-redux";
import { applyMiddleware, combineReducers, compose, createStore } from "redux";
import thunk from "redux-thunk";
import * as Base from "./BaseStore";

export type ApplicationState = {
    base: Base.BaseState;
};

const reducers = {
    base: Base.reducer,
};

export default function configureStore(history: any, initialState?: any) {
    const middleware = [thunk, routerMiddleware(history)];

    // In development, use the browser's Redux dev tools extension if installed
    const enhancers = [];
    const isDevelopment = process.env.NODE_ENV === "development";
    if (
        isDevelopment &&
        typeof window !== "undefined" &&
        (window as any).devToolsExtension
    ) {
        enhancers.push((window as any).devToolsExtension());
    }

    const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer,
    });

    return createStore(
        rootReducer,
        initialState,
        compose(applyMiddleware(...middleware), ...enhancers)
    );
}

export interface ThunkDispatch<TBasicAction, TExtraThunkArg> {
    <TReturnType>(
        thunkAction: AppThunkAction<TBasicAction, TReturnType, TExtraThunkArg>
    ): TReturnType;
    <A extends TBasicAction>(action: A): A;
    <TAction extends TBasicAction, TReturnType>(
        action:
            | TAction
            | AppThunkAction<TBasicAction, TReturnType, TExtraThunkArg>
    ): TAction | TReturnType;
}

export interface AppThunkAction<
    TAction,
    TReturnType = void,
    TExtraThunkArg = never
> {
    (
        dispatch: ThunkDispatch<TAction, TExtraThunkArg>,
        getState: () => ApplicationState,
        extraArguments?: TExtraThunkArg
    ): TReturnType;
}

export type AsMapObject<K> = { [U in keyof K]: K[U] };
