import { Outlet } from 'react-router';

import Header from '../components/Layout/Header';


const Layout = () => {


  return (
    <div style={styles.container}>
      <Header />
       
      
      

      <div style={styles.content}>
        <Outlet />
      </div>
      
    </div>
  );
};

const styles = {
  container: {
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
