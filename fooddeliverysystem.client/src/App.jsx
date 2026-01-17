// src/App.jsx
import React, { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import './App.css';

// Components
import Navbar from './components/Navbar';
import AdminNavbar from './components/AdminNavbar';
import MobileMenu from './components/MobileMenu';
import ProductModal from './components/ProductModal';
import CreateAdminAcc from './g1/CreateAdminAcc';
import ManageUsers from './components/ManageUsers';
import Profile from './components/Profile'; // ADD THIS

// Pages
import Home from './pages/Home';
import Home2 from './pages/Home2';
import Menu from './pages/Menu';
import AdminMenu from './pages/AdminMenu';
import Login from './pages/Login';
import Register from './pages/Register';

// Data & API
import { PRODUCTS } from './data';
import { api } from './services/api';

const CATEGORY_LABELS = {
    classic: 'Classic Coffee Series',
    frappe: 'Frappe',
    latte: 'Latte',
    specialty: 'Specialty Drinks',
    baked: 'Cupcakes & Baked Goods',
    snacks: 'Snacks'
};

function AppContent() {
    const { user, logout } = useAuth();
    const [currentPage, setCurrentPage] = useState('home');
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [selectedProduct, setSelectedProduct] = useState(null);
    const [showCreateAdmin, setShowCreateAdmin] = useState(false);
    const [showProfile, setShowProfile] = useState(false); // ADD THIS
    const [menuItems, setMenuItems] = useState(PRODUCTS);

    useEffect(() => {
        let mounted = true;

        const loadData = async () => {
            const data = await api.fetchProducts();
            if (data && mounted) {
                setMenuItems(data);
            }
        };

        loadData();

        return () => {
            mounted = false;
        };
    }, []);

    const handleRefreshData = async () => {
        const data = await api.fetchProducts();
        if (data) {
            setMenuItems(data);
        }
    };

    const handleViewProduct = (product) => {
        let categoryName = 'Signature Series';
        if (product.category) {
            categoryName = product.category;
        } else {
            for (const [key, items] of Object.entries(menuItems)) {
                if (key !== 'bestsellers' && Array.isArray(items)) {
                    if (items.some(item => item.id === product.id)) {
                        categoryName = CATEGORY_LABELS[key] || key;
                        break;
                    }
                }
            }
        }
        setSelectedProduct({ ...product, category: categoryName });
    };

    const handleCloseModal = () => setSelectedProduct(null);

    const getRelatedItems = () => {
        if (!selectedProduct) return [];
        const categoryKeys = Object.keys(menuItems).filter(key => key !== 'bestsellers');
        for (const key of categoryKeys) {
            const categoryItems = menuItems[key];
            if (Array.isArray(categoryItems) && categoryItems.some(item => item.id === selectedProduct.id)) {
                return categoryItems;
            }
        }
        return [];
    };

    const navigateTo = (page, e) => {
        if (e) e.preventDefault();

        // ADD THIS - Handle profile navigation
        if (page === 'profile') {
            setShowProfile(true);
            return;
        }

        if (page === 'create_admin') {
            if (user && user.role === 'superadmin') {
                setShowCreateAdmin(true);
            } else {
                alert('Only Super Admins can create admin accounts');
            }
        } else if (page === 'create_rider') {
            if (user && (user.role === 'superadmin' || user.role === 'admin')) {
                setShowCreateAdmin(true);
            } else {
                alert('Only Admins can create rider accounts');
            }
        } else if (page === 'manage_users') {
            if (user && (user.role === 'superadmin' || user.role === 'admin')) {
                setCurrentPage('manage_users');
                window.scrollTo(0, 0);
            } else {
                alert('Only Admins can manage users');
            }
        } else if (page === 'logout') {
            logout();
            setCurrentPage('home');
        } else if (page === 'admin') {
            if (user && (user.role === 'admin' || user.role === 'superadmin')) {
                setCurrentPage(page);
                window.scrollTo(0, 0);
            } else {
                alert('Please login as Admin or Super Admin to access this page');
                setCurrentPage('login');
            }
        } else {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };

    const renderContent = () => {
        const isAdmin = user && (user.role === 'admin' || user.role === 'superadmin');

        if ((currentPage === 'admin' || currentPage === 'manage_users') && !isAdmin) {
            return (
                <Login
                    setCurrentPage={setCurrentPage}
                    onBackToHome={() => setCurrentPage('home')}
                    onLoginSuccess={(userData) => {
                        if (userData && (userData.role === 'admin' || userData.role === 'superadmin')) {
                            setCurrentPage('home');
                        } else {
                            setCurrentPage('home');
                        }
                    }}
                />
            );
        }

        switch (currentPage) {
            case 'home':
                if (isAdmin) {
                    return (
                        <Home2
                            products={menuItems}
                            onViewProduct={handleViewProduct}
                            onOrderNow={(e) => navigateTo('menu', e)}
                        />
                    );
                } else {
                    return (
                        <Home
                            onNavigateToRegister={() => setCurrentPage('register')}
                            onNavigateToLogin={() => setCurrentPage('login')}
                        />
                    );
                }

            case 'login':
                return (
                    <Login
                        setCurrentPage={setCurrentPage}
                        onBackToHome={() => setCurrentPage('home')}
                        onLoginSuccess={(userData) => {
                            if (userData && (userData.role === 'admin' || userData.role === 'superadmin')) {
                                setCurrentPage('home');
                            } else {
                                setCurrentPage('home');
                            }
                        }}
                    />
                );

            case 'register':
                return (
                    <Register
                        onBackToHome={() => setCurrentPage('home')}
                        onSwitchToLogin={() => setCurrentPage('login')}
                    />
                );

            case 'admin':
                return (
                    <AdminMenu
                        products={menuItems}
                        onSave={handleRefreshData}
                        onViewProduct={handleViewProduct}
                    />
                );

            case 'manage_users':
                return (
                    <ManageUsers
                        onBack={() => setCurrentPage('home')}
                    />
                );

            case 'menu':
                return (
                    <Menu
                        products={menuItems}
                        onViewProduct={handleViewProduct}
                    />
                );

            default:
                return <Menu products={menuItems} onViewProduct={handleViewProduct} />;
        }
    };

    const isAdmin = currentPage === 'admin' && user && (user.role === 'admin' || user.role === 'superadmin');
    const isAuthPage = currentPage === 'login' || currentPage === 'register';
    const isHomePage = currentPage === 'home' && (!user || (user.role !== 'admin' && user.role !== 'superadmin'));
    const isManageUsersPage = currentPage === 'manage_users';

    return (
        <div className="app-container">
            {!isAuthPage && !isHomePage && !isManageUsersPage && (
                <>
                    {isAdmin ? (
                        <AdminNavbar
                            currentPage={currentPage}
                            navigateTo={navigateTo}
                        />
                    ) : (
                        <Navbar
                            currentPage={currentPage}
                            navigateTo={navigateTo}
                            onOpenMobileMenu={() => setIsMobileMenuOpen(true)}
                            user={user}
                        />
                    )}

                    <MobileMenu
                        isOpen={isMobileMenuOpen}
                        onClose={() => setIsMobileMenuOpen(false)}
                        navigateTo={navigateTo}
                    />
                </>
            )}

            {renderContent()}

            {!isAuthPage && (
                <>
                    <ProductModal
                        product={selectedProduct}
                        onClose={handleCloseModal}
                        relatedItems={getRelatedItems()}
                        onViewProduct={handleViewProduct}
                    />

                    {showCreateAdmin && user && (user.role === 'admin' || user.role === 'superadmin') && (
                        <CreateAdminAcc onClose={() => setShowCreateAdmin(false)} />
                    )}

                    {/* ADD THIS - Profile Modal */}
                    {showProfile && user && (
                        <Profile
                            isOpen={showProfile}
                            onClose={() => setShowProfile(false)}
                        />
                    )}
                </>
            )}
        </div>
    );
}

function App() {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    );
}

export default App;