import { css as apCss, CSSProperties, StyleSheet } from "aphrodite";

export function css(cssProperties: CSSProperties) {
    return apCss(StyleSheet.create({ c: cssProperties }).c);
}
