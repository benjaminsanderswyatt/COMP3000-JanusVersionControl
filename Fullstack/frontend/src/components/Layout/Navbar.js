import { useNavigate } from 'react-router';

const Navbar = () => {
  const navigate = useNavigate();
  const token = localStorage.getItem('token'); // Check for token (user is logged in)

  return (
    <nav style={styles.navbar}>
      



      {token ? (
          <>
            <ul style={styles.navbarLinks}>
              <button style={styles.navbarItem} onClick={() => navigate("/repositories")}>Repositories</button>
            </ul>
            <div style={styles.navbarSpacer}></div>
            <ul style={styles.navbarLinks}>
              <button style={styles.navbarItem} onClick={() => navigate("/account")}>Account</button>
            </ul>
          </>
        ) : (
          <>
            <ul style={styles.navbarLinks}>
              <button style={styles.navbarItem} onClick={() => navigate("/repositories")}>Login</button>
            </ul>
          </>
        )
      }
    </nav>
  );
};

const styles = {
    navbar: {
      display: "flex",
      backgroundColor: "white",
      padding: "0px 20px",
      height: "50px",
      margin: "0",
      gap: "2px",
      listStyle: "none",
      justifyContent: "left",
      alignItems: "center",
      borderBottom: '#d9d9d9 solid 1px',
    },
    navbarLinks: {
      padding: "0px",
      margin: "0px",
      height: "100%",
      width: "100%",
      minWidth: "15vw"
    },
    navbarItem: {
      cursor: "pointer",
      height: "100%",
      width: "100%",
      border: "none",
      background: "none",
      color: "black",
    },
    navbarSpacer: {
      width: "1px",
      height: "24px",
      backgroundColor: "grey",
      margin: "0 0px",
    },
  };

export default Navbar;
