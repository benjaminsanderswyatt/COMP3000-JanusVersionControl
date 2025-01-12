import { useNavigate } from 'react-router';
import { useAuth } from '../../contexts/AuthContext';

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
      {isLoggedIn ? ( 
        <div>{authUser} - Logged In they are</div>
      ): (<div>{authUser} - Not logged in</div>)};

      <div style={styles.holder}>
        <button style={styles.buttonLogin} onClick={() => navigate("/login")}>Login</button>
        <button style={styles.buttonRegister} onClick={() => navigate('/register')}>Register</button>
      </div>

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
  holder: {
    display: 'flex',
    marginLeft: 'auto',
    gap: '12px',
    height: '32px',
  },
  buttonLogin: {
    backgroundColor: 'var(--secondary)',
    borderRadius: '8px',
    border: 'var(--border) solid 1px',
    width: '83px',
    color: 'black',
    fontSize: '0.9rem',
    cursor: 'pointer',
  },
  buttonRegister: {
    backgroundColor: 'var(--primary)',
    color: 'white',
    borderRadius: '8px',
    border: 'var(--border) solid 1px',
    width: '83px',
    fontSize: '0.9rem',
    cursor: 'pointer',
  },
};

export default Header;
