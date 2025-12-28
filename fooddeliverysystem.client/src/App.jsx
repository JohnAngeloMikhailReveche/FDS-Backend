import { useState } from 'react';
import Home from './pages/Home';
import Register from './pages/Register';
import Login from "./pages/Login";

function App() {
    const [currentPage, setCurrentPage] = useState('home'); // 'home', 'register', or 'login'

    if (currentPage === 'register') {
        return <Register onBackToHome={() => setCurrentPage('home')} />;
    }

    if (currentPage === 'login') {
        return <Login setCurrentPage={setCurrentPage} />;
    }

    return <Home onNavigateToRegister={() => setCurrentPage('register')} onNavigateToLogin={() => setCurrentPage('login')} />;
}

export default App;