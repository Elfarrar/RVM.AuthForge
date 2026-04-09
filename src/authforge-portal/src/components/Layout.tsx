import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import Navbar from './Navbar';

export default function Layout() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="app">
      {isAuthenticated && <Navbar />}
      <main className="main-content">
        <Outlet />
      </main>
      <footer className="footer">
        {!isAuthenticated && (
          <p>
            <Link to="/login">Login</Link> | <Link to="/register">Register</Link>
          </p>
        )}
      </footer>
    </div>
  );
}
