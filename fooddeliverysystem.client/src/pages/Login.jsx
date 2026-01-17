// src/pages/Login.jsx
import { useState } from 'react';
import "../css/Login.css";
import { loginUser, resetPassword } from '../services/authService';
import { useAuth } from '../context/AuthContext';

// Import images - Comment out if files don't exist
import kapebaraLogo from "../images/kapebara-logo-transparent.png";
import capybaraImage from "../images/mascot.png";

function Login({ setCurrentPage, onBackToHome, onLoginSuccess }) {
    const [internalPage, setInternalPage] = useState('login');

    const handleBackToHome = () => {
        if (onBackToHome) {
            onBackToHome();
        } else if (setCurrentPage) {
            setCurrentPage('home');
        }
    };

    if (internalPage === 'forgotpassword') {
        return <ForgotPasswordPage setInternalPage={setInternalPage} kapebaraLogo={kapebaraLogo} />;
    }

    return <LoginPage
        setInternalPage={setInternalPage}
        handleBackToHome={handleBackToHome}
        setCurrentPage={setCurrentPage}
        kapebaraLogo={kapebaraLogo}
        capybaraImage={capybaraImage}
        onLoginSuccess={onLoginSuccess}
    />;
}

function LoginPage({ setInternalPage, handleBackToHome, setCurrentPage, kapebaraLogo, capybaraImage, onLoginSuccess }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const { login } = useAuth();

    const handleSubmit = async () => {
        if (!email || !password) {
            alert('Please enter both email and password');
            return;
        }

        setIsLoading(true);

        try {
            const response = await loginUser(email, password);

            let userRole, userName, token, userEmail;

            if (response.user) {
                userRole = response.user.role;
                userName = response.user.fullName;
                token = response.user.token;
                userEmail = response.user.email;
            } else {
                userRole = response.role;
                userName = response.fullName;
                token = response.token;
                userEmail = response.email;
            }

            // Normalize role to lowercase for consistency
            const normalizedRole = userRole.toLowerCase();

            // Create user data object
            const userData = {
                email: userEmail,
                fullName: userName,
                role: normalizedRole // 'superadmin', 'admin', or 'user'
            };

            // Store in memory via AuthContext
            login(token, userData);

            alert(`✅ Welcome back, ${userName}!\n\nRole: ${userRole}`);

            // Call onLoginSuccess if provided (from App.jsx)
            if (onLoginSuccess) {
                onLoginSuccess(userData);
            } else {
                // Fallback: Navigate based on role
                if (normalizedRole === 'superadmin' || normalizedRole === 'admin') {
                    if (setCurrentPage) {
                        setCurrentPage('home'); // Will show Home2 for admins
                    }
                } else {
                    if (handleBackToHome) {
                        handleBackToHome();
                    }
                }
            }

        } catch (error) {
            console.error('Login failed:', error);
            alert(`Login failed: ${error.message}`);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="fullScreen">
            <div className="loginGrid">
                <div className="leftColumn">
                    {kapebaraLogo ? (
                        <img src={kapebaraLogo} alt="Kapebara Logo" className="logoLeft" />
                    ) : (
                        <div style={{ padding: '20px', background: '#f0f0f0', textAlign: 'center' }}>
                            <h2>KAPEBARA</h2>
                        </div>
                    )}
                    {capybaraImage && (
                        <img src={capybaraImage} alt="Capybara" className="capybaraImage" />
                    )}
                </div>

                <div className="rightColumn">
                    <h1 className="loginHeader">LOG IN</h1>
                    <h2 className="welcomeText">Welcome back!</h2>
                    <p className="formInfo">Enter your credentials to continue</p>

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
                                disabled={isLoading}
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
                                onKeyPress={(e) => e.key === 'Enter' && handleSubmit()}
                                disabled={isLoading}
                            />
                        </div>

                        <button onClick={handleSubmit} className="submitBtn" disabled={isLoading}>
                            {isLoading ? 'Logging in...' : 'Submit'}
                        </button>

                        <div className="forgotPassword">
                            <span onClick={() => setInternalPage('forgotpassword')} className="forgotPasswordLink">
                                Forgot Password
                            </span>
                        </div>

                        <div className="backToHome">
                            <span onClick={handleBackToHome} className="backLink">
                                ← Back to Home
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

function ForgotPasswordPage({ setInternalPage, kapebaraLogo }) {
    const [formData, setFormData] = useState({
        email: '',
        newPassword: '',
        confirmPassword: ''
    });
    const [isLoading, setIsLoading] = useState(false);

    const handleChange = (field, value) => {
        setFormData({ ...formData, [field]: value });
    };

    const handleSubmit = async () => {
        if (!formData.email || !formData.newPassword || !formData.confirmPassword) {
            alert('Please fill in all fields');
            return;
        }

        if (formData.newPassword !== formData.confirmPassword) {
            alert('Passwords do not match!');
            return;
        }

        if (formData.newPassword.length < 6) {
            alert('Password must be at least 6 characters long');
            return;
        }

        setIsLoading(true);

        try {
            await resetPassword(formData.email, formData.newPassword, formData.confirmPassword);
            alert('✅ Password reset successful!\n\nYou can now login with your new password.');
            setInternalPage('login');
        } catch (error) {
            alert(`❌ Password reset failed:\n${error.message}`);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="fullScreen">
            <div className="forgotContainer">
                {kapebaraLogo ? (
                    <img src={kapebaraLogo} alt="Kapebara Logo" className="logoCenter" />
                ) : (
                    <div style={{ padding: '20px', background: '#f0f0f0', textAlign: 'center', marginBottom: '20px' }}>
                        <h2>KAPEBARA</h2>
                    </div>
                )}
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
                            disabled={isLoading}
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
                            disabled={isLoading}
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
                            disabled={isLoading}
                        />
                    </div>

                    <button onClick={handleSubmit} className="submitBtn" disabled={isLoading}>
                        {isLoading ? 'Resetting...' : 'Submit'}
                    </button>

                    <div className="backToLogin">
                        <span onClick={() => setInternalPage('login')} className="backLink">
                            Back to Login
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Login;