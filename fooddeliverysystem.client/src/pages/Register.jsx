import React, { useState } from "react";
import { useForm } from "react-hook-form";
import "./Register.css";
import smallIcon from "./kapebara.png";
import mascotIllustration from "./kapebaralogo.png";

export default function Register() {
    const { register, handleSubmit, watch, formState: { errors } } = useForm();
    const [showPassword, setShowPassword] = useState(false);
    const password = watch("password");

    const onSubmit = (data) => {
        const { confirmPass: _, ...formDataToSend } = data;
        console.log("Registration Successful! Data:", formDataToSend);
        alert("Registration successful! Welcome to Kapebara!");
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
                        />
                        {errors.email && <p className="error-message">{errors.email.message}</p>}
                    </div>

                    <div className="input-group password-group">
                        <label>Password</label>
                        <div style={{ position: 'relative' }}>
                            <input
                                type={showPassword ? "text" : "password"}
                                placeholder="Enter your password..."
                                {...register("password", {
                                    required: "Password is required.",
                                    minLength: { value: 8, message: "Password must be at least 8 characters." }
                                })}
                                className={errors.password ? "input-error" : ""}
                                style={{ paddingRight: '40px' }}
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                style={{
                                    position: 'absolute',
                                    right: '10px',
                                    top: '50%',
                                    transform: 'translateY(-50%)',
                                    background: 'none',
                                    border: 'none',
                                    cursor: 'pointer',
                                    fontSize: '18px',
                                    color: '#4a3832'
                                }}
                            >
                                {showPassword ? '👁️' : '👁️‍🗨️'}
                            </button>
                        </div>
                        {errors.password && <p className="error-message">{errors.password.message}</p>}
                    </div>

                    <div className="input-group password-group">
                        <label>Confirm Password</label>
                        <div style={{ position: 'relative' }}>
                            <input
                                type={showPassword ? "text" : "password"}
                                placeholder="Confirm your password..."
                                {...register("confirmPass", {
                                    required: "Confirm Password is required.",
                                    validate: value => value === password || "Passwords do not match."
                                })}
                                className={errors.confirmPass ? "input-error" : ""}
                                style={{ paddingRight: '40px' }}
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                style={{
                                    position: 'absolute',
                                    right: '10px',
                                    top: '50%',
                                    transform: 'translateY(-50%)',
                                    background: 'none',
                                    border: 'none',
                                    cursor: 'pointer',
                                    fontSize: '18px',
                                    color: '#4a3832'
                                }}
                            >
                                {showPassword ? '👁️' : '👁️‍🗨️'}
                            </button>
                        </div>
                        {errors.confirmPass && <p className="error-message">{errors.confirmPass.message}</p>}
                    </div>

                    <button type="submit" className="submit-btn">Submit</button>
                </form>
            </div>

            <div className="register-right">
                <img src={smallIcon} alt="Kapebara Small Icon" className="kapebara-small-icon" />
                <img src={mascotIllustration} alt="Kapebara Mascot Illustration" className="kapebara-mascot" />
                <p className="login-text">
                    Already have an account? <a href="#">Log in here</a>
                </p>
            </div>
        </div>
    );
}