const Header = () => {

    return (
        <div style={styles.header}>
        <h1>Janus Version Control</h1>
        </div>
    );
};

const styles = {
    header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "10px 20px",
    backgroundColor: "#52a5de",
    color: "black",
    minHeight: "12vh",
    },
};

export default Header;
  