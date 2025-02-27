import { Outlet } from 'react-router';

import Header from '../components/layout/Header';

// Styles
import styles from "../styles/pages/Layout.module.css";

const Layout = () => {


  return (
    <div className={styles.container}>
      <Header />
      
      

      <div className={styles.content}>
        <Outlet />
      </div>
      
    </div>
  );
};

export default Layout;
