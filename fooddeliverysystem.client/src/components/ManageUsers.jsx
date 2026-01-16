import React, { useState, useEffect } from 'react';
import '../css/ManageUsers.css';

const ManageUsers = ({ onBack }) => {
    const [users, setUsers] = useState([]);
    const [filteredUsers, setFilteredUsers] = useState([]);
    const [selectedRole, setSelectedRole] = useState('all');
    const [searchTerm, setSearchTerm] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = async () => {
        setIsLoading(true);
        try {
            const token = localStorage.getItem('authToken');

            if (!token) {
                throw new Error('No authentication token found. Please login again.');
            }

            const response = await fetch('https://localhost:7164/api/auth/users', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Session expired. Please login again.');
                } else if (response.status === 403) {
                    throw new Error('You don\'t have permission to view users.');
                }
                throw new Error('Failed to fetch users');
            }

            const data = await response.json();
            const usersList = data.users || [];

            setUsers(usersList);
            setFilteredUsers(usersList);
        } catch (error) {
            console.error('Error fetching users:', error);
            alert(`Failed to load users:\n\n${error.message}`);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        let filtered = users;

        if (selectedRole !== 'all') {
            filtered = filtered.filter(user =>
                user.role.toLowerCase() === selectedRole.toLowerCase()
            );
        }

        if (searchTerm) {
            filtered = filtered.filter(user =>
                user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                user.email.toLowerCase().includes(searchTerm.toLowerCase())
            );
        }

        setFilteredUsers(filtered);
    }, [selectedRole, searchTerm, users]);

    const toggleUserStatus = async (userId, userEmail, userName, currentStatus) => {
        const action = currentStatus ? 'deactivate' : 'activate';

        if (!window.confirm(`Are you sure you want to ${action} ${userName}?`)) {
            return;
        }

        try {
            const token = localStorage.getItem('authToken');

            if (!token) {
                alert('❌ No authentication token found. Please login again.');
                return;
            }

            const response = await fetch('https://localhost:7164/api/auth/toggle-status', {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email: userEmail })
            });

            // Try to parse the response body
            let responseData;
            const contentType = response.headers.get('content-type');

            console.log('Response status:', response.status);
            console.log('Content-Type:', contentType);

            try {
                if (contentType && contentType.includes('application/json')) {
                    responseData = await response.json();
                } else {
                    // If response is not JSON, get it as text
                    const textResponse = await response.text();
                    console.log('Text response:', textResponse);
                    responseData = { message: textResponse || `HTTP ${response.status}: ${response.statusText}` };
                }
            } catch (parseError) {
                console.error('Error parsing response:', parseError);
                responseData = { message: `HTTP ${response.status}: ${response.statusText}` };
            }

            console.log('Response data:', responseData);

            if (!response.ok) {
                // Handle specific error cases
                if (response.status === 401) {
                    alert('❌ Session expired. Please login again.');
                    return;
                } else if (response.status === 403) {
                    alert('❌ You don\'t have permission to perform this action.');
                    return;
                } else if (response.status === 500) {
                    alert(`❌ Server Error (500):\n\n${responseData.message || 'Internal server error occurred'}`);
                    return;
                }

                throw new Error(responseData.message || 'Failed to toggle user status');
            }

            // Success - update the user in the local state
            setUsers(users.map(user =>
                user.id === userId
                    ? { ...user, isActive: responseData.isActive }
                    : user
            ));

            alert(`✅ ${userName} has been ${responseData.isActive ? 'activated' : 'deactivated'} successfully.`);

        } catch (error) {
            console.error('Error toggling user status:', error);
            alert(`❌ Failed to toggle user status:\n\n${error.message}`);
        }
    };

    const deleteUser = async (userId, userName, userEmail) => {
        if (!window.confirm(`Are you sure you want to delete ${userName}?\n\nThis action cannot be undone.`)) {
            return;
        }

        try {
            const token = localStorage.getItem('authToken');

            if (!token) {
                alert('❌ No authentication token found. Please login again.');
                return;
            }

            const response = await fetch('https://localhost:7164/api/auth/delete', {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email: userEmail })
            });

            let responseData;
            const contentType = response.headers.get('content-type');

            if (contentType && contentType.includes('application/json')) {
                responseData = await response.json();
            } else {
                const textResponse = await response.text();
                responseData = { message: textResponse || `HTTP ${response.status}: ${response.statusText}` };
            }

            if (!response.ok) {
                if (response.status === 401) {
                    alert('❌ Session expired. Please login again.');
                    return;
                } else if (response.status === 403) {
                    alert('❌ You don\'t have permission to perform this action.');
                    return;
                }

                throw new Error(responseData.message || 'Failed to delete user');
            }

            setUsers(users.filter(user => user.id !== userId));
            alert(`✅ ${userName} has been deleted successfully.`);

        } catch (error) {
            console.error('Error deleting user:', error);
            alert(`❌ Failed to delete user:\n\n${error.message}`);
        }
    };

    return (
        <div className="manage-users-container">
            <div className="manage-users-header">
                <button onClick={onBack} className="back-btn">
                    Back
                </button>
                <h1>Manage Users</h1>
            </div>

            <div className="filters-section">
                <div className="filter-group">
                    <label>Filter by Role:</label>
                    <select
                        value={selectedRole}
                        onChange={(e) => setSelectedRole(e.target.value)}
                        className="role-filter"
                    >
                        <option value="all">All Users</option>
                        <option value="customer">Customers</option>
                        <option value="rider">Riders</option>
                        <option value="admin">Admins</option>
                        <option value="superadmin">Super Admins</option>
                    </select>
                </div>

                <div className="filter-group">
                    <label>Search:</label>
                    <input
                        type="text"
                        placeholder="Search by name or email..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="search-input"
                    />
                </div>
            </div>

            <div className="user-count">
                Showing {filteredUsers.length} of {users.length} users
            </div>

            {isLoading ? (
                <div className="loading">Loading users...</div>
            ) : (
                <div className="users-table-container">
                    <table className="users-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Status</th>
                                <th>Created Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredUsers.length === 0 ? (
                                <tr>
                                    <td colSpan="6" style={{ textAlign: 'center', padding: '30px', color: '#6B5441' }}>
                                        No users found
                                    </td>
                                </tr>
                            ) : (
                                filteredUsers.map((user) => (
                                    <tr key={user.id}>
                                        <td>{user.fullName}</td>
                                        <td>{user.email}</td>
                                        <td>
                                            <span className={`role-badge role-${user.role.toLowerCase()}`}>
                                                {user.role}
                                            </span>
                                        </td>
                                        <td>
                                            <span className={`status-badge status-${user.isActive ? 'active' : 'inactive'}`}>
                                                {user.isActive ? 'Active' : 'Inactive'}
                                            </span>
                                        </td>
                                        <td>
                                            {user.createdAt
                                                ? new Date(user.createdAt).toLocaleDateString()
                                                : 'N/A'}
                                        </td>
                                        <td>
                                            <div className="action-buttons">
                                                <button
                                                    onClick={() => toggleUserStatus(user.id, user.email, user.fullName, user.isActive)}
                                                    className={`action-btn ${user.isActive ? 'deactivate-btn' : 'activate-btn'}`}
                                                    title={user.isActive ? 'Deactivate' : 'Activate'}
                                                >
                                                    {user.isActive ? '🚫' : '✅'}
                                                </button>
                                                <button
                                                    onClick={() => deleteUser(user.id, user.fullName, user.email)}
                                                    className="action-btn delete-btn"
                                                    title="Delete User"
                                                >
                                                    🗑️
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default ManageUsers;