import AnimatedBackground from "../components/home/AnimatedBackground";

// Styles
import styles from "../styles/pages/home/Home.module.css";

const Home = () => {
  return (
    <div className={styles.main}>
      <AnimatedBackground />
      <div className={styles.content}>
        <h1 className={styles.text}>Janus Version Control</h1>
      </div>
    </div>
  );
};

export default Home;
