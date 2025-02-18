import { useNavigate } from 'react-router';
import { useAuth } from '../../contexts/AuthContext';
import Navbar from './Navbar';
import LogButtonsBar from './LogButtonsBar';

import styles from "../../styles/Components/Layout/Header.module.css";


const Header = () => {
  const navigate = useNavigate();
  const { authUser, isLoggedIn } = useAuth();

  return (
    <header className={styles.header}>
      <img src="/logo.svg"
       alt="Logo"
       className={styles.logo}
       onClick={() => navigate("/")}
      />

      {isLoggedIn ? <Navbar authUser={authUser} /> : <LogButtonsBar />}
    </header>
  );
};


export default Header;
