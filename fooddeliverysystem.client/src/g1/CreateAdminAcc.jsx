// src/g1/createadminacc.jsx
import React, { useState, useEffect } from "react";
import { useAuth } from '../context/AuthContext';
import "./createadminacc.css";
import mascot from "./mascot.png";

const CreateAdminAcc = ({ onClose }) => {
    const { token } = useAuth();
    const [showRider, setShowRider] = useState(false);
    const [isLoadingAdmin, setIsLoadingAdmin] = useState(false);
    const [isLoadingRider, setIsLoadingRider] = useState(false);

    const [adminData, setAdminData] = useState({
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
    });

    const [riderData, setRiderData] = useState({
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
        contactNumber: "",
        motorcycleModel: "",
        plateNumber: "",
    });

    useEffect(() => {
        const esc = (e) => e.key === "Escape" && onClose();
        window.addEventListener("keydown", esc);
        return () => window.removeEventListener("keydown", esc);
    }, [onClose]);

    const handleAdminChange = (e) =>
        setAdminData({ ...adminData, [e.target.name]: e.target.value });

    const handleRiderChange = (e) =>
        setRiderData({ ...riderData, [e.target.name]: e.target.value });

    const submitAdmin = async (e) => {
        e.preventDefault();

        if (adminData.password !== adminData.confirmPassword) {
            alert("Admin passwords do not match");
            return;
        }

        if (adminData.password.length < 8) {
            alert("Password must be at least 8 characters");
            return;
        }

        setIsLoadingAdmin(true);

        try {
            const response = await fetch('https://localhost:7164/api/auth/create/admin', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(adminData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to create admin');
            }

            const data = await response.json();
            alert(`? Admin account created successfully!\n\nName: ${data.admin.fullName}\nEmail: ${data.admin.email}`);

            setAdminData({ fullName: "", email: "", password: "", confirmPassword: "" });
            onClose();

        } catch (error) {
            console.error('Error creating admin:', error);
            alert(`? Failed to create admin: ${error.message}`);
        } finally {
            setIsLoadingAdmin(false);
        }
    };

    const submitRider = async (e) => {
        e.preventDefault();

        if (riderData.password !== riderData.confirmPassword) {
            alert("Rider passwords do not match");
            return;
        }

        if (riderData.password.length < 8) {
            alert("Password must be at least 8 characters");
            return;
        }

        setIsLoadingRider(true);

        try {
            const response = await fetch('https://localhost:7164/api/auth/create/rider', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(riderData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to create rider');
            }

            const data = await response.json();
            alert(`? Rider account created successfully!\n\nName: ${data.rider.fullName}\nEmail: ${data.rider.email}\nPlate: ${riderData.plateNumber}`);

            setRiderData({
                fullName: "",
                email: "",
                password: "",
                confirmPassword: "",
                contactNumber: "",
                motorcycleModel: "",
                plateNumber: "",
            });

            setShowRider(false);

        } catch (error) {
            console.error('Error creating rider:', error);
            alert(`? Failed to create rider: ${error.message}`);
        } finally {
            setIsLoadingRider(false);
        }
    };

    return (
        <div className="ca-modal-overlay">
            <div className="ca-admin-container">
                <button className="ca-main-close" onClick={onClose}>
                    &times;
                </button>

                <button className="rider-hover-btn" onClick={() => setShowRider(true)}>
                    Rider Accounts
                </button>

                <h1>Admin Account Management</h1>

                <img src={mascot} alt="Mascot" className="mascot-img" />

                <div className="ca-account-card">
                    <h2>Create Admin Account</h2>
                    <form onSubmit={submitAdmin}>
                        <input
                            name="fullName"
                            placeholder="Full Name"
                            value={adminData.fullName}
                            onChange={handleAdminChange}
                            disabled={isLoadingAdmin}
                            required
                        />
                        <input
                            name="email"
                            type="email"
                            placeholder="Email"
                            value={adminData.email}
                            onChange={handleAdminChange}
                            disabled={isLoadingAdmin}
                            required
                        />
                        <input
                            name="password"
                            type="password"
                            placeholder="Password (min 8 characters)"
                            value={adminData.password}
                            onChange={handleAdminChange}
                            disabled={isLoadingAdmin}
                            minLength={8}
                            required
                        />
                        <input
                            name="confirmPassword"
                            type="password"
                            placeholder="Confirm Password"
                            value={adminData.confirmPassword}
                            onChange={handleAdminChange}
                            disabled={isLoadingAdmin}
                            required
                        />
                        <button type="submit" disabled={isLoadingAdmin}>
                            {isLoadingAdmin ? 'Creating...' : 'Create Admin'}
                        </button>
                    </form>
                </div>

                {showRider && (
                    <div className="ca-modal-overlay" onClick={() => setShowRider(false)} style={{ zIndex: 11001 }}>
                        <div className="ca-modal" onClick={(e) => e.stopPropagation()}>
                            <button className="ca-modal-close" onClick={() => setShowRider(false)}>
                                &times;
                            </button>

                            <div className="ca-account-card">
                                <h2>Create Rider Account</h2>
                                <form onSubmit={submitRider}>
                                    <input
                                        name="fullName"
                                        placeholder="Full Name"
                                        value={riderData.fullName}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                        required
                                    />
                                    <input
                                        name="email"
                                        type="email"
                                        placeholder="Email"
                                        value={riderData.email}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                        required
                                    />
                                    <input
                                        name="contactNumber"
                                        placeholder="Contact Number"
                                        value={riderData.contactNumber}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                    />
                                    <input
                                        name="motorcycleModel"
                                        placeholder="Motorcycle Model"
                                        value={riderData.motorcycleModel}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                    />
                                    <input
                                        name="plateNumber"
                                        placeholder="Plate Number"
                                        value={riderData.plateNumber}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                    />
                                    <input
                                        name="password"
                                        type="password"
                                        placeholder="Password (min 8 characters)"
                                        value={riderData.password}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                        minLength={8}
                                        required
                                    />
                                    <input
                                        name="confirmPassword"
                                        type="password"
                                        placeholder="Confirm Password"
                                        value={riderData.confirmPassword}
                                        onChange={handleRiderChange}
                                        disabled={isLoadingRider}
                                        required
                                    />
                                    <button type="submit" disabled={isLoadingRider}>
                                        {isLoadingRider ? 'Creating...' : 'Create Rider'}
                                    </button>
                                </form>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default CreateAdminAcc;