import { Outlet, Link } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import '../styles/pages/Layout.css';

const Layout = () => {
  const navigate = useNavigate();
  const token = localStorage.getItem('token'); // Check for token (user is logged in)

  const handleLogout = () => {
    // Remove token from localStorage
    localStorage.removeItem('token');
    navigate('/');
  };

  return (
    <>
      <nav className="navbar">
        <ul className="navbar-links">
          {!token ? (

            // Show Login link when the user is not logged in
            <li className="navbar-item">
              <Link to="/" className="navbar-link">Login</Link>
            </li>

          ) : (

            // Show Repos link and Logout when the user is logged in
            <>
              <li className="navbar-item">
                <Link to="/repos" className="navbar-link">Repos</Link>
              </li>
              <li className="navbar-item">
                <button onClick={handleLogout} className="navbar-link logout-button">
                  Logout
                </button>
              </li>
            </>
            
          )}
        </ul>
      </nav>

      <Outlet />
    </>
  )
};

export default Layout;
