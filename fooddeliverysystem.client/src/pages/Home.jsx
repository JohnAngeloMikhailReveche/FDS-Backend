import React from "react";
import "../css/Home.css";
// Images
import titleImage from "../images/kapebara-logo-transparent.png";
import leftImage from "../images/mascot.png";
import drink1 from "../images/classic-coffeebara.png";
import drink2 from "../images/classic-creamybara.png";
import drink3 from "../images/classic-kapebarako-1.png";
import footerImage from "../images/FooterLogo.png";
import instagramIcon from "../images/instagram.png";
import facebookIcon from "../images/facebook.png";
import tiktokIcon from "../images/tiktok.png";

function Home({ onNavigateToRegister, onNavigateToLogin }) {
    const drinks = [
        {
            id: 1,
            image: drink1,
            name: "coffeebara",
            description: "A coffee drink typically made from equal parts espresso...",
            rating: 4.9
        },
        {
            id: 2,
            image: drink2,
            name: "creamybara",
            description: "Black coffee is a hot coffee beverage simply made...",
            rating: 4.9
        },
        {
            id: 3,
            image: drink3,
            name: "kapebarako",
            description: "A coffee drink known for its chocolate sweet, rich, and...",
            rating: 4.9
        }
    ];

    return (
        <div className="onboarding-container">
            {/* Navbar */}
            <nav className="navbar">
                <ul>
                    <li onClick={onNavigateToLogin} style={{ cursor: 'pointer' }}>Log In</li>
                    <li onClick={onNavigateToRegister} style={{ cursor: 'pointer' }}>Sign Up</li>
                    <li>About Us</li>
                </ul>
            </nav>

            {/* Hero Section */}
            <section className="section section-1">
                <div className="left-content">
                    <img src={titleImage} alt="Kapebara Title" className="title-image" />
                    <p className="description">
                        Where every cup, every flavor, and every sip is an invitation
                        to slow down and enjoy life like a capybara.
                    </p>
                    <button className="primary-btn">Order Now</button>
                </div>
                <div className="right-image-container">
                    <img src={leftImage} alt="Mascot" className="left-image" />
                </div>
            </section>

            {/* Featured Drinks */}
            <section className="section section-2">
                <h2 className="title">Featured Drinks</h2>
                <div className="drinks-container">
                    {drinks.map((drink) => (
                        <div key={drink.id} className="drink-card">
                            <img src={drink.image} alt={drink.name} className="drink-img" />
                            <h3 className="drink-name">{drink.name}</h3>
                            <p className="drink-description">{drink.description}</p>
                            <p className="drink-rating">⭐ {drink.rating}</p>
                            <button className="more-btn">More</button>
                        </div>
                    ))}
                </div>
            </section>

            {/* About Us */}
            <section className="section section-3">
                <h2 className="title">About Us</h2>
                <p className="subtitle">
                    Kapebara Coffee is a place to slow down, savor your coffee, and enjoy life.
                    We craft each drink with care, from the freshest ingredients and finest beans.
                </p>
            </section>

            {/* Footer */}
            <footer className="footer">
                <div className="footer-top">
                    {/* LEFT SIDE */}
                    <div className="footer-left">
                        <img
                            src={footerImage}
                            alt="Kapebara Coffee Logo"
                            className="footer-logo"
                        />

                        <ul className="footer-links">
                            <li>Menu</li>
                            <li>About Us</li>
                            <li>Terms of Use</li>
                        </ul>
                    </div>
                </div>

                {/* BOTTOM SECTION */}
                <div className="footer-bottom">
                    <p className="footer-text">
                        Slow down. Sip gently. Enjoy the moment.
                    </p>

                    <p className="footer-copy">
                        © {new Date().getFullYear()} Kapebara. All rights reserved.
                    </p>

                    {/* RIGHT SIDE – SOCIAL MEDIA */}
                    <div className="footer-socials">
                        <a href="#">
                            <img src={instagramIcon} alt="Instagram" />
                        </a>
                        <a href="#">
                            <img src={tiktokIcon} alt="Tiktok" />
                        </a>
                        <a href="#">
                            <img src={facebookIcon} alt="Facebook" />
                        </a>
                    </div>
                </div>
            </footer>
        </div>
    );
}

export default Home;