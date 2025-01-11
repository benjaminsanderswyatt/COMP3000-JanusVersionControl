const NoPage = () => {
  return (
    <div style={styles.main}>
      <h1>Error: 404</h1>
      <h3>Could not find page</h3>
    </div>
  );
};

const styles = {
  main: {
    justifyItems: 'center',
  }
}

export default NoPage;
