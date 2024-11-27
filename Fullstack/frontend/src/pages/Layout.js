import { Outlet, Link } from "react-router-dom";
import '../styles/pages/Layout.css';

const Layout = () => {
  return (
    <>
      <nav className="navbar">
        <ul className="navbar-links">
          <li className="navbar-item">
            <Link to="/" className="navbar-link">Home</Link>
          </li>
          <li className="navbar-item">
            <Link to="/login" className="navbar-link">Login</Link>
          </li>
          {/* You can add more links here as needed */}
        </ul>
      </nav>

      <Outlet />
    </>
  )
};

export default Layout;
