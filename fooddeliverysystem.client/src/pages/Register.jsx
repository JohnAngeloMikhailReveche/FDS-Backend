// src/pages/Register.jsx
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import "../css/Register.css";
import { registerCustomer } from '../services/authService';
import { useAuth } from '../context/AuthContext';

// Import images - use the same logo from Login
import smallIcon from "../images/kapebara-logo-transparent.png";
import mascotIllustration from "../images/mascot.png";

export default function Register({ onBackToHome, onSwitchToLogin }) {
    const { register, handleSubmit, watch, formState: { errors } } = useForm();
    const { login } = useAuth();
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const password = watch("password");

    const onSubmit = async (data) => {
        setIsLoading(true);

        try {
            // Register as customer (backend determines role via /register/customer endpoint)
            const response = await registerCustomer(data);

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

            // Log the role for debugging
            console.log('Registration response:', response);
            console.log('User role returned:', userRole);

            // Verify that the registered user is indeed a customer
            // Check for both 'customer' and 'Customer' (case-insensitive)
            const normalizedRole = userRole?.toLowerCase();
            if (normalizedRole !== 'customer') {
                console.error('Unexpected role returned:', userRole);
                throw new Error(`Registration failed: Unexpected role '${userRole}'. Only customer accounts can be created.`);
            }

            // Store in memory via AuthContext
            login(token, {
                email: userEmail,
                fullName: userName,
                role: userRole
            });

            alert(`✅ Registration successful! Welcome ${userName}!`);

            // Redirect to home
            if (onBackToHome) {
                onBackToHome();
            }

        } catch (error) {
            console.error('Registration failed:', error);
            alert(`Registration failed: ${error.message}`);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="register-wrapper">
            <div className="register-left">
                <h1 className="signup-title">SIGN UP</h1>
                <h3 className="welcome">Welcome to Kapebara!</h3>
                <form onSubmit={handleSubmit(onSubmit)} className="form-box">

                    <div className="input-group">
                        <label>Name</label>
                        <input
                            type="text"
                            placeholder="Enter your name..."
                            {...register("fullName", { required: "Name is required." })}
                            className={errors.fullName ? "input-error" : ""}
                            disabled={isLoading}
                        />
                        {errors.fullName && <p className="error-message">{errors.fullName.message}</p>}
                    </div>

                    <div className="input-group">
                        <label>Contact No.</label>
                        <input
                            type="text"
                            placeholder="Enter your contact number..."
                            {...register("contact", {
                                required: "Contact number is required.",
                                pattern: { value: /^\d{11}$/, message: "Must be a valid 11-digit number." }
                            })}
                            className={errors.contact ? "input-error" : ""}
                            disabled={isLoading}
                        />
                        {errors.contact && <p className="error-message">{errors.contact.message}</p>}
                    </div>

                    <div className="input-group">
                        <label>Email Address</label>
                        <input
                            type="email"
                            placeholder="Enter your email address..."
                            {...register("email", {
                                required: "Email is required.",
                                pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Please enter a valid email address." }
                            })}
                            className={errors.email ? "input-error" : ""}
                            disabled={isLoading}
                        />
                        {errors.email && <p className="error-message">{errors.email.message}</p>}
                    </div>

                    <div className="input-group">
                        <label>Password</label>
                        <div className="password-container">
                            <input
                                type={showPassword ? "text" : "password"}
                                placeholder="Enter your password..."
                                {...register("password", {
                                    required: "Password is required.",
                                    minLength: { value: 8, message: "Password must be at least 8 characters." }
                                })}
                                className={errors.password ? "input-error" : ""}
                                disabled={isLoading}
                            />
                            <i
                                className={`fa-solid ${showPassword ? "fa-eye-slash" : "fa-eye"} toggle-password-icon`}
                                onClick={() => setShowPassword(!showPassword)}
                            ></i>
                        </div>
                        {errors.password && <p className="error-message">{errors.password.message}</p>}
                    </div>

                    <div className="input-group">
                        <label>Confirm Password</label>
                        <div className="password-container">
                            <input
                                type={showConfirmPassword ? "text" : "password"}
                                placeholder="Confirm your password..."
                                {...register("confirmPass", {
                                    required: "Confirm Password is required.",
                                    validate: value => value === password || "Passwords do not match."
                                })}
                                className={errors.confirmPass ? "input-error" : ""}
                                disabled={isLoading}
                            />
                            <i
                                className={`fa-solid ${showConfirmPassword ? "fa-eye-slash" : "fa-eye"} toggle-password-icon`}
                                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            ></i>
                        </div>
                        {errors.confirmPass && <p className="error-message">{errors.confirmPass.message}</p>}
                    </div>

                    <button type="submit" className="submit-btn" disabled={isLoading}>
                        {isLoading ? 'Registering...' : 'Submit'}
                    </button>
                </form>
            </div>

            <div className="register-right">
                {smallIcon ? (
                    <img src={smallIcon} alt="Kapebara Small Icon" className="kapebara-small-icon" />
                ) : (
                    <div style={{ padding: '20px', background: '#f0f0f0', textAlign: 'center' }}>
                        <h2>KAPEBARA</h2>
                    </div>
                )}
                {mascotIllustration && (
                    <img src={mascotIllustration} alt="Kapebara Mascot Illustration" className="kapebara-mascot" />
                )}
                <p className="login-text">
                    Already have an account? <a onClick={onSwitchToLogin} style={{ cursor: 'pointer', color: '#ffffff', textDecoration: 'underline' }}>Log in here</a>
                </p>
            </div>
        </div>
    );
}