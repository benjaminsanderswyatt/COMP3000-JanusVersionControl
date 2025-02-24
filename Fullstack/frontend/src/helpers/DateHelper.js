export const DateType = {
  RELATIVE: "RELATIVE", // 2 days ago
  DATE_ONLY: "DATE_ONLY", // 19 May 2025
  TIME_ONLY: "TIME_ONLY", // 15:45
  FULL: "FULL", // 19 May 2025, 15:45
};



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
    console.error("Error formatting date: ", error);

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
    console.error("Error formatting date: ", error);

    return "";
  }
};

export const formatOnlyTime = (dateString) => {
  try {
    const date = new Date(dateString);

    if (isNaN(date.getTime())) {
      throw new Error("Invalid date string");
    }

    return new Intl.DateTimeFormat('en-GB', { 
      timeStyle: 'short' 
    }).format(date);

  } catch (error) {
    console.error("Error formatting date: ", error);

    return "";
  }
};



export const formatRelativeTime = (dateString) => {
  try {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) {
      throw new Error("Invalid date string");
    }

    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);

    const intervals = {
      year: 31536000,
      month: 2592000,
      week: 604800,
      day: 86400,
      hour: 3600,
      minute: 60,
      second: 1,
    };

    for (const [unit, seconds] of Object.entries(intervals)) {
      const interval = Math.floor(diffInSeconds / seconds);
      if (interval >= 1) {
        const rtf = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });
        return rtf.format(-interval, unit);
      }
    }

    return "Just now";
    
  } catch (error) {
    console.error("Error formatting relative time: ", error);

    return "";
  }
};