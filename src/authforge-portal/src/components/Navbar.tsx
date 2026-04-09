import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <div className="navbar-brand">AuthForge</div>
      <div className="navbar-links">
        <NavLink to="/profile">Profile</NavLink>
        <NavLink to="/change-password">Password</NavLink>
        <NavLink to="/2fa">2FA</NavLink>
      </div>
      <div className="navbar-right">
        <span className="navbar-user">{user?.fullName}</span>
        <button className="btn-logout" onClick={() => void logout()}>Logout</button>
      </div>
    </nav>
  );
}
