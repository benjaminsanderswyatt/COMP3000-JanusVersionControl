import React from "react";

const LoginForm = ({ formData, onChange, onSubmit }) => (
  <form onSubmit={onSubmit}>
    <div className="form-group">
      <label htmlFor="email">Email</label>
      <input
        type="email"
        id="email"
        name="email"
        value={formData.email}
        onChange={onChange}
        required
      />
    </div>
    <div className="form-group">
      <label htmlFor="password">Password</label>
      <input
        type="password"
        id="password"
        name="password"
        value={formData.password}
        onChange={onChange}
        required
      />
    </div>
    <button type="submit">Login</button>
  </form>
);

export default LoginForm;