import { useState } from 'react';
import './Login.css';
// Import images
import kapebaraLogo from './images/kapebara-logo-transparent.png';
import capybaraImage from './images/mascot.png';

function Login({ setCurrentPage }) {
    const [internalPage, setInternalPage] = useState('login'); // 'login' or 'forgotpassword'

    const handleBackToHome = () => {
        if (setCurrentPage) {
            setCurrentPage('home');
        }
    };

    if (internalPage === 'forgotpassword') {
        return <ForgotPasswordPage setInternalPage={setInternalPage} />;
    }

    return <LoginPage setInternalPage={setInternalPage} handleBackToHome={handleBackToHome} />;
}

// Login Page Component
function LoginPage({ setInternalPage, handleBackToHome }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    const handleSubmit = () => {
        console.log('Form submitted', { email, password });
    };

    return (
        <div className="fullScreen">
            <div className="loginGrid">
                <div className="leftColumn">
                    <img src={kapebaraLogo} alt="Kapebara Logo" className="logoLeft" />
                    <img src={capybaraImage} alt="Capybara sipping drink" className="capybaraImage" />
                </div>

                <div className="rightColumn">
                    <h1 className="loginHeader">LOG IN</h1>
                    <h2 className="welcomeText">Welcome back, [Name]!</h2>
                    <p className="formInfo">You are logging in as an Admin.</p>

                    <div className="loginForm">
                        <div className="formGroup">
                            <label htmlFor="email" className="label">Email Address</label>
                            <input
                                type="email"
                                id="email"
                                placeholder="Enter your email address..."
                                className="input"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                        </div>

                        <div className="formGroup">
                            <label htmlFor="password" className="label">Password</label>
                            <input
                                type="password"
                                id="password"
                                placeholder="Enter your password..."
                                className="input"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                        </div>

                        <button onClick={handleSubmit} className="submitBtn">Submit</button>

                        <div className="forgotPassword">
                            <span
                                onClick={() => setInternalPage('forgotpassword')}
                                className="forgotPasswordLink"
                            >
                                Forgot Password
                            </span>
                        </div>

                        <div className="riderLogin">
                            <p className="riderLoginText">
                                Log in as rider instead?
                                <span className="riderLoginLink"> Click here.</span>
                            </p>
                        </div>

                        <div className="backToHome">
                            <span
                                onClick={handleBackToHome}
                                className="backLink"
                            >
                                ← Back to Home
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

// Forgot Password Page Component
function ForgotPasswordPage({ setInternalPage }) {
    const [formData, setFormData] = useState({
        email: '',
        newPassword: '',
        confirmPassword: ''
    });

    const handleChange = (field, value) => {
        setFormData({
            ...formData,
            [field]: value
        });
    };

    const handleSubmit = () => {
        if (formData.newPassword !== formData.confirmPassword) {
            alert('Passwords do not match!');
            return;
        }

        console.log('Form submitted', formData);
        alert('Password reset successful!');
    };

    return (
        <div className="fullScreen">
            <div className="forgotContainer">
                <img src={kapebaraLogo} alt="Kapebara Logo" className="logoCenter" />
                <h1 className="pageHeader">FORGOT PASSWORD</h1>

                <div className="forgotForm">
                    <div className="formGroup">
                        <label htmlFor="forgot-email" className="label">Email</label>
                        <input
                            type="email"
                            id="forgot-email"
                            placeholder="Enter your email..."
                            className="input"
                            value={formData.email}
                            onChange={(e) => handleChange('email', e.target.value)}
                        />
                    </div>

                    <div className="formGroup">
                        <label htmlFor="new-password" className="label">New Password</label>
                        <input
                            type="password"
                            id="new-password"
                            placeholder="Enter your new password..."
                            className="input"
                            value={formData.newPassword}
                            onChange={(e) => handleChange('newPassword', e.target.value)}
                        />
                    </div>

                    <div className="formGroup">
                        <label htmlFor="confirm-password" className="label">Confirm Password</label>
                        <input
                            type="password"
                            id="confirm-password"
                            placeholder="Confirm your password..."
                            className="input"
                            value={formData.confirmPassword}
                            onChange={(e) => handleChange('confirmPassword', e.target.value)}
                        />
                    </div>

                    <button onClick={handleSubmit} className="submitBtn">Submit</button>

                    <div className="backToLogin">
                        <span
                            onClick={() => setInternalPage('login')}
                            className="backLink"
                        >
                            Back to Login
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Login;