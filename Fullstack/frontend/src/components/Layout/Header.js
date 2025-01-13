import { useNavigate } from 'react-router';
import { useAuth } from '../../contexts/AuthContext';
import Navbar from './Navbar';
import LogButtonsBar from './LogButtonsBar';


const Header = () => {
  const navigate = useNavigate();
  const { authUser, isLoggedIn } = useAuth();

  return (
    <header style={styles.header}>
      <img src="/logo.png"
       alt="Logo"
       style={styles.logo}
       onClick={() => navigate("/")}
      />

      {isLoggedIn ? <Navbar authUser={authUser} /> : <LogButtonsBar />}
    </header>
  );
};

const styles = {
  header: {
    display: "flex",
    backgroundColor: "var(--card)",
    padding: "0px 20px",
    height: "70px",
    margin: "0",
    gap: "2px",
    listStyle: "none",
    justifyContent: "left",
    alignItems: "center",
    borderBottom: 'var(--border) solid 1px',
  },
  logo: {
    width: 'auto',
    height: '45px',
    cursor: 'pointer',
  },
};

export default Header;
