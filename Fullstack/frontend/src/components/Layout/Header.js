import { useNavigate } from 'react-router';
import { useAuth } from '../../contexts/AuthContext';
import Navbar from './Navbar';

import styles from "../../styles/components/layout/Header.module.css";


const Header = () => {
  const navigate = useNavigate();
  const { authUser, authUserId, isLoggedIn } = useAuth();

  return (
    <header className={styles.header}>
      <img src="/logo.svg"
       alt="Logo"
       className={styles.logo}
       onClick={() => navigate("/")}
      />

      <Navbar authUser={authUser} authUserId={authUserId} isLoggedIn={isLoggedIn} />
      
    </header>
  );
};

export default Header;
