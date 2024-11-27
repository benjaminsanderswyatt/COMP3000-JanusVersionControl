import React from "react";

const ToggleButton = ({ isRegistering, onClick }) => (
  <button className="toggle-button" onClick={onClick}>
    {isRegistering
      ? "Already have an account? Login"
      : "Don't have an account? Register"}
  </button>
);

export default ToggleButton;