import AnimatedBackground from "../components/Home/AnimatedBackground";

// Styles
import "../styles/Pages/Home/Home.css";

const Home = () => {
  return (
    <div className="main">
      <AnimatedBackground />
      <div className="content">
        <h1 className="text">Janus Version Control</h1>
      </div>
    </div>
  );
};

export default Home;
