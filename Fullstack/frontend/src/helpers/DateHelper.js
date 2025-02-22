export const formatDate = (dateString) => {
  try {
    const date = new Date(dateString);

    if (isNaN(date.getTime())) {
      throw new Error("Invalid date string");
    }

    return new Intl.DateTimeFormat('en-GB', { 
      dateStyle: 'medium', 
      timeStyle: 'short' 
    }).format(date);

  } catch (error) {
    console.error("Error formatting date:", error);

    return "";
  }
};

export const formatOnlyDate = (dateString) => {
  try {
    const date = new Date(dateString);

    if (isNaN(date.getTime())) {
      throw new Error("Invalid date string");
    }

    return new Intl.DateTimeFormat('en-GB', { 
      dateStyle: 'medium'
    }).format(date);

  } catch (error) {
    console.error("Error formatting date:", error);

    return "";
  }
};