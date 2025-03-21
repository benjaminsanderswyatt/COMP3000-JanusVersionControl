export const AccessLevel = {
  READ: 'READ',
  WRITE: 'WRITE',
  ADMIN: 'ADMIN',
  OWNER: 'OWNER'
};

export const accessLevelMapping = {
  READ: 0,
  WRITE: 1,
  ADMIN: 2,
  OWNER: 3,
};

export const displayAccessLevel = (level) => {
  switch (level) {
    case 0:
      return "Read";
    case 1:
      return "Write";
    case 2:
      return "Admin";
    // case 3:
    //   return "Owner";
    default:
      return "Unknown";
  }
};