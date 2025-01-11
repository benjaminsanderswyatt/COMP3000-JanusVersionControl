import { Outlet, useNavigate } from 'react-router';

import Navbar from '../components/Layout/Navbar';


const Layout = () => {
  const token = localStorage.getItem('token'); // Check for token (user is logged in)


  return (
    <div style={styles.constainer}>
      <Navbar />
       
      
      

      <div style={styles.content}>
        <Outlet />
      </div>
      
    </div>
  );
};

const styles = {
  constainer: {
    backgroundColor: 'var(--background)',
    minHeight: "100vh",
    display: "flex",
    flexDirection: "column"
  },
  content: {
    display: "flex",
    justifyContent: "center",
    flex: '1 1 auto',
  }



}

export default Layout;
