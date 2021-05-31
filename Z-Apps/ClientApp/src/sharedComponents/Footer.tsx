import { css, StyleSheet } from "aphrodite";
import * as React from "react";
import "./Footer.css";

const styles = StyleSheet.create({
    sp: {
        "@media (max-width: 600px)": {
            marginTop: 8,
        },
    },
});

export default function Footer() {
    return (
        <footer className="footer">
            <div className="center">
                <div className="container text-muted">
                    Copyright{" "}
                    <a href="https://www.lingual-ninja.com/developer">
                        Kosuke Zaizen
                    </a>
                    . All rights reserved.{" "}
                    <a
                        href="https://www.lingual-ninja.com/terms"
                        style={{ display: "inline-block" }}
                        className={css(styles.sp)}
                    >
                        Terms of Use
                    </a>
                </div>
            </div>
        </footer>
    );
}
