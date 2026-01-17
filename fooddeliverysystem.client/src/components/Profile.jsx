import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { changePassword, updateUserProfile, getUserProfile } from '../services/authService';
import '../css/Profile.css';

const Profile = ({ isOpen, onClose }) => {
    const { user, updateUser } = useAuth(); // ADD updateUser here
    const [isClosing, setIsClosing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [activeTab, setActiveTab] = useState('info'); // 'info' or 'password'

    const [formData, setFormData] = useState({
        fullName: '',
        email: '',
        phoneNumber: '',
        address: ''
    });

    const [passwordData, setPasswordData] = useState({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
    });

    const [errors, setErrors] = useState({});
    const [successMessage, setSuccessMessage] = useState('');

    useEffect(() => {
        if (user && isOpen) {
            // Load fresh profile data from backend
            const loadProfile = async () => {
                try {
                    const profileData = await getUserProfile();
                    setFormData({
                        fullName: profileData.fullName || '',
                        email: profileData.email || '',
                        phoneNumber: profileData.phoneNumber || '',
                        address: profileData.address || ''
                    });
                } catch (error) {
                    console.error('Failed to load profile:', error);
                    // Fallback to user context data
                    setFormData({
                        fullName: user.fullName || '',
                        email: user.email || '',
                        phoneNumber: user.phoneNumber || '',
                        address: user.address || ''
                    });
                }
            };

            loadProfile();
        }
    }, [user, isOpen]);

    if (!isOpen && !isClosing) return null;

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(() => {
            setIsClosing(false);
            setErrors({});
            setSuccessMessage('');
            setPasswordData({
                currentPassword: '',
                newPassword: '',
                confirmPassword: ''
            });
            onClose();
        }, 300);
    };

    const validateProfileForm = () => {
        const newErrors = {};

        if (!formData.fullName.trim()) {
            newErrors.fullName = 'Full name is required';
        } else if (formData.fullName.trim().length < 2) {
            newErrors.fullName = 'Name must be at least 2 characters';
        }

        // Email validation removed - field is read-only

        if (formData.phoneNumber && !/^(\+63|0)[0-9]{10}$/.test(formData.phoneNumber.replace(/\s/g, ''))) {
            newErrors.phoneNumber = 'Invalid Philippine phone number (e.g., 09171234567)';
        }

        if (formData.address && formData.address.trim().length < 10) {
            newErrors.address = 'Address must be at least 10 characters';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const validatePasswordForm = () => {
        const newErrors = {};

        if (!passwordData.currentPassword) {
            newErrors.currentPassword = 'Current password is required';
        }

        if (!passwordData.newPassword) {
            newErrors.newPassword = 'New password is required';
        } else if (passwordData.newPassword.length < 8) {
            newErrors.newPassword = 'Password must be at least 8 characters';
        } else if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(passwordData.newPassword)) {
            newErrors.newPassword = 'Password must contain uppercase, lowercase, and number';
        }

        if (!passwordData.confirmPassword) {
            newErrors.confirmPassword = 'Please confirm your password';
        } else if (passwordData.newPassword !== passwordData.confirmPassword) {
            newErrors.confirmPassword = 'Passwords do not match';
        }

        if (passwordData.currentPassword === passwordData.newPassword) {
            newErrors.newPassword = 'New password must be different from current password';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleProfileSubmit = async (e) => {
        e.preventDefault();

        if (!validateProfileForm()) return;

        setIsLoading(true);
        setSuccessMessage('');
        setErrors({});

        try {
            // Call the real API
            await updateUserProfile(formData);

            setSuccessMessage('Profile updated successfully!');

            setTimeout(() => {
                setSuccessMessage('');
            }, 3000);

        } catch (error) {
            setErrors({ submit: error.message || 'Failed to update profile' });
        } finally {
            setIsLoading(false);
        }
    };

    const handlePasswordSubmit = async (e) => {
        e.preventDefault();

        if (!validatePasswordForm()) return;

        setIsLoading(true);
        setSuccessMessage('');
        setErrors({});

        try {
            // Call the real API
            await changePassword(
                passwordData.currentPassword,
                passwordData.newPassword,
                passwordData.confirmPassword
            );

            setSuccessMessage('Password updated successfully!');
            setPasswordData({
                currentPassword: '',
                newPassword: '',
                confirmPassword: ''
            });

            setTimeout(() => {
                setSuccessMessage('');
            }, 3000);

        } catch (error) {
            setErrors({ submit: error.message || 'Failed to update password' });
        } finally {
            setIsLoading(false);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        // Clear error for this field when user types
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    const handlePasswordChange = (e) => {
        const { name, value } = e.target;
        setPasswordData(prev => ({ ...prev, [name]: value }));
        // Clear error for this field when user types
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    return (
        <div
            className={`profile-overlay ${isOpen && !isClosing ? 'open' : 'closing'}`}
            onClick={handleClose}
        >
            <div
                className={`profile-modal ${isOpen && !isClosing ? 'open' : 'closing'}`}
                onClick={(e) => e.stopPropagation()}
            >
                {/* Header */}
                <div className="profile-header">
                    <h2>My Profile</h2>
                    <button className="profile-close-btn" onClick={handleClose}>
                        &times;
                    </button>
                </div>

                {/* User Info Display */}
                <div className="profile-user-info">
                    <div className="profile-avatar">
                        {user?.fullName?.charAt(0).toUpperCase() || 'U'}
                    </div>
                    <div className="profile-user-details">
                        <h3>{user?.fullName}</h3>
                        <p className="profile-role-badge">{user?.role || 'Customer'}</p>
                    </div>
                </div>

                {/* Tabs */}
                <div className="profile-tabs">
                    <button
                        className={`profile-tab ${activeTab === 'info' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveTab('info');
                            setErrors({});
                            setSuccessMessage('');
                        }}
                    >
                        Personal Info
                    </button>
                    <button
                        className={`profile-tab ${activeTab === 'password' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveTab('password');
                            setErrors({});
                            setSuccessMessage('');
                        }}
                    >
                        Change Password
                    </button>
                </div>

                {/* Success Message */}
                {successMessage && (
                    <div className="profile-success-message">
                        ✓ {successMessage}
                    </div>
                )}

                {/* Error Message */}
                {errors.submit && (
                    <div className="profile-error-message">
                        ✗ {errors.submit}
                    </div>
                )}

                {/* Tab Content */}
                <div className="profile-content">
                    {activeTab === 'info' ? (
                        <form onSubmit={handleProfileSubmit} className="profile-form">
                            <div className="profile-form-group">
                                <label htmlFor="fullName">Full Name *</label>
                                <input
                                    type="text"
                                    id="fullName"
                                    name="fullName"
                                    value={formData.fullName}
                                    onChange={handleInputChange}
                                    className={errors.fullName ? 'error' : ''}
                                    placeholder="Enter your full name"
                                />
                                {errors.fullName && <span className="error-text">{errors.fullName}</span>}
                            </div>

                            <div className="profile-form-group">
                                <label htmlFor="email">Email *</label>
                                <input
                                    type="email"
                                    id="email"
                                    name="email"
                                    value={formData.email}
                                    onChange={handleInputChange}
                                    className={errors.email ? 'error' : ''}
                                    placeholder="Enter your email"
                                    disabled
                                    style={{ backgroundColor: '#f5f5f5', cursor: 'not-allowed' }}
                                />
                                <small style={{ color: '#888', fontSize: '0.85rem' }}>
                                    Email cannot be changed
                                </small>
                            </div>

                            <div className="profile-form-group">
                                <label htmlFor="phoneNumber">Phone Number</label>
                                <input
                                    type="tel"
                                    id="phoneNumber"
                                    name="phoneNumber"
                                    value={formData.phoneNumber}
                                    onChange={handleInputChange}
                                    className={errors.phoneNumber ? 'error' : ''}
                                    placeholder="09171234567"
                                />
                                {errors.phoneNumber && <span className="error-text">{errors.phoneNumber}</span>}
                            </div>

                            <div className="profile-form-group">
                                <label htmlFor="address">Delivery Address</label>
                                <textarea
                                    id="address"
                                    name="address"
                                    value={formData.address}
                                    onChange={handleInputChange}
                                    className={errors.address ? 'error' : ''}
                                    placeholder="Enter your complete address"
                                    rows="3"
                                />
                                {errors.address && <span className="error-text">{errors.address}</span>}
                            </div>

                            <button
                                type="submit"
                                className="profile-submit-btn"
                                disabled={isLoading}
                            >
                                {isLoading ? 'Updating...' : 'Save Changes'}
                            </button>
                        </form>
                    ) : (
                        <form onSubmit={handlePasswordSubmit} className="profile-form">
                            <div className="profile-form-group">
                                <label htmlFor="currentPassword">Current Password *</label>
                                <input
                                    type="password"
                                    id="currentPassword"
                                    name="currentPassword"
                                    value={passwordData.currentPassword}
                                    onChange={handlePasswordChange}
                                    className={errors.currentPassword ? 'error' : ''}
                                    placeholder="Enter current password"
                                />
                                {errors.currentPassword && <span className="error-text">{errors.currentPassword}</span>}
                            </div>

                            <div className="profile-form-group">
                                <label htmlFor="newPassword">New Password *</label>
                                <input
                                    type="password"
                                    id="newPassword"
                                    name="newPassword"
                                    value={passwordData.newPassword}
                                    onChange={handlePasswordChange}
                                    className={errors.newPassword ? 'error' : ''}
                                    placeholder="Enter new password (min 8 characters)"
                                />
                                {errors.newPassword && <span className="error-text">{errors.newPassword}</span>}
                                <small className="password-hint">
                                    Must contain uppercase, lowercase, and number
                                </small>
                            </div>

                            <div className="profile-form-group">
                                <label htmlFor="confirmPassword">Confirm New Password *</label>
                                <input
                                    type="password"
                                    id="confirmPassword"
                                    name="confirmPassword"
                                    value={passwordData.confirmPassword}
                                    onChange={handlePasswordChange}
                                    className={errors.confirmPassword ? 'error' : ''}
                                    placeholder="Confirm new password"
                                />
                                {errors.confirmPassword && <span className="error-text">{errors.confirmPassword}</span>}
                            </div>

                            <button
                                type="submit"
                                className="profile-submit-btn"
                                disabled={isLoading}
                            >
                                {isLoading ? 'Updating...' : 'Update Password'}
                            </button>
                        </form>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Profile;