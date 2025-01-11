import AnimatedBackground from "../components/Home/AnimatedBackground";


const Home = () => {
  return (
    <div style={styles.main}>
      <AnimatedBackground />
      <div style={styles.content}>
        <h1 style={styles.text}>Janus Version Control</h1>
      </div>
    </div>
  );
};

const styles = {
  main: {
    position: "relative",
    width: "100%",
    overflow: "hidden",
  },
  content: {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    textAlign: "center",
    fontSize: '2rem',
    width: '100%',
    padding: '30px',
  },
  text: {
    color: 'white',
    textShadow: '-2px 2px 4px black'
  }
};

export default Home;
