import styled from 'styled-components';

const convertStylesToString = (styles) => {
  return Object.entries(styles)
    .map(([key, value]) => `${key.replace(/([A-Z])/g, '-$1').toLowerCase()}: ${value};`) // Convert camelCase to kebab-case
    .join(' ');
};

// Generic Styled Button with hover
const StyledButton = styled.button`
  background: ${({ bgColor }) => bgColor || "var(--button)"};
  transition: background 0.3s ease;
  ${({ customStyles }) => customStyles};

  &:hover {
    background: ${({ noHover, bgColor, hoverColor }) =>
      noHover ? bgColor : hoverColor || "var(--buttonhover)"};
  }
`;

const Button = ({ style, onClick, disabled, noHover, children }) => {
  const { backgroundColor, hoverColor, ...otherStyles } = style || {};

  const customStyles = convertStylesToString(otherStyles);

  return (
    <StyledButton
      bgColor={backgroundColor}
      hoverColor={hoverColor}
      customStyles={customStyles}
      onClick={onClick}
      disabled={disabled}
      noHover={noHover}
    >
      {children}
    </StyledButton>
  );
};

export default Button;