import "bootstrap/dist/css/bootstrap.css";
import * as React from "react";
import { Link } from "react-router-dom";
import Collapse from "reactstrap/lib/Collapse";
import Container from "reactstrap/lib/Container";
import Navbar from "reactstrap/lib/Navbar";
import NavbarBrand from "reactstrap/lib/NavbarBrand";
import NavbarToggler from "reactstrap/lib/NavbarToggler";
import NavLink from "reactstrap/lib/NavLink";
import "./NavMenu.css";

function a(props: any) {
    return <a href={props.to} {...props} />;
}

function NavigationItems(props: { closeToggle: () => void }) {
    let objLinks = {
        "Japanese Folktales": "https://www.lingual-ninja.com/folktales",
        "Hiragana / Katakana":
            "https://www.lingual-ninja.com/hiragana-katakana",
        Vocabulary: "https://www.lingual-ninja.com/vocabulary-list",
        Articles: "https://www.lingual-ninja.com/articles",
        "Action Games": "https://www.lingual-ninja.com/ninja",
    } as const;
    let linkList = [];
    for (let key in objLinks) {
        linkList.push(
            <NavLink
                key={key}
                tag={a}
                className="text-light dropdown"
                to={objLinks[key as keyof typeof objLinks]}
            >
                {key}
            </NavLink>
        );
    }
    return (
        <ul className="navbar-nav flex-grow" onClick={props.closeToggle}>
            {linkList}
        </ul>
    );
}

interface OuterProps {}

type InnerProps = OuterProps;

class NavMenu extends React.Component<
    InnerProps,
    {
        isOpen: boolean;
    }
> {
    constructor(props: InnerProps) {
        super(props);

        this.toggle = this.toggle.bind(this);
        this.closeToggle = this.closeToggle.bind(this);
        this.state = {
            isOpen: false,
        };
    }
    toggle() {
        this.setState({
            isOpen: !this.state.isOpen,
        });
    }
    closeToggle() {
        this.setState({
            isOpen: false,
        });
    }
    render() {
        return (
            <header>
                <Navbar
                    variant="pills"
                    className="navbar-inverse navbar-expand-md navbar-toggleable-md border-bottom box-shadow mb-3"
                    style={{ backgroundColor: "#222222" }}
                >
                    <Container>
                        <NavbarBrand tag={Link} to="/">
                            <span
                                style={{
                                    whiteSpace: "nowrap",
                                    fontWeight: "bold",
                                }}
                                className="z-apps-title text-light"
                                onClick={this.closeToggle}
                            >
                                Pages about Japan
                            </span>
                        </NavbarBrand>
                        <NavbarToggler onClick={this.toggle} className="mr-2" />
                        <Collapse
                            className="d-md-inline-flex flex-md-row-reverse"
                            isOpen={this.state.isOpen}
                            navbar
                        >
                            <NavigationItems closeToggle={this.closeToggle} />
                        </Collapse>
                    </Container>
                </Navbar>
            </header>
        );
    }
}

export default NavMenu;
