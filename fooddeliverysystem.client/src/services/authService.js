// src/services/authService.js
const API_BASE_URL = 'https://localhost:7164/api';

export const loginUser = async (email, password) => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Login failed');
    }

    const data = await response.json();

    // Store token in localStorage immediately after successful login
    if (data.user && data.user.token) {
        localStorage.setItem('authToken', data.user.token);
        localStorage.setItem('user', JSON.stringify({
            email: data.user.email,
            fullName: data.user.fullName,
            role: data.user.role.toLowerCase()
        }));
        console.log('✅ Token saved to localStorage');
    }

    return data;
};

export const registerCustomer = async (customerData) => {
    const response = await fetch(`${API_BASE_URL}/auth/register/customer`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            fullName: customerData.fullName,
            email: customerData.email,
            password: customerData.password,
            confirmPassword: customerData.confirmPass,
            phoneNumber: customerData.contact || '',
            address: ''
        }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Registration failed');
    }

    return await response.json();
};

export const resetPassword = async (email, newPassword, confirmPassword) => {
    const response = await fetch(`${API_BASE_URL}/auth/reset-password`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            email,
            newPassword,
            confirmPassword
        }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Password reset failed');
    }

    return await response.json();
};

export const changePassword = async (currentPassword, newPassword, confirmPassword) => {
    const token = localStorage.getItem('authToken');

    if (!token) {
        throw new Error('No authentication token found. Please login again.');
    }

    const response = await fetch(`${API_BASE_URL}/auth/change-password`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
            currentPassword,
            newPassword,
            confirmPassword
        }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Password change failed');
    }

    return await response.json();
};

export const getUserProfile = async () => {
    const token = localStorage.getItem('authToken');

    if (!token) {
        throw new Error('No authentication token found. Please login again.');
    }

    const response = await fetch(`${API_BASE_URL}/auth/profile`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Failed to fetch profile');
    }

    return await response.json();
};

// FIXED: Update user profile - NO EMAIL in request
export const updateUserProfile = async (profileData) => {
    const token = localStorage.getItem('authToken');

    if (!token) {
        throw new Error('No authentication token found. Please login again.');
    }

    const requestBody = {
        fullName: profileData.fullName,
        phoneNumber: profileData.phoneNumber || null,
        address: profileData.address || null
        // NO EMAIL - backend uses JWT token to identify user
    };

    const response = await fetch(`${API_BASE_URL}/auth/update-profile`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Profile update failed');
    }

    return await response.json();
};