export const formatDate = (dateString) => {
    
    const date = new Date(dateString);

    return new Intl.DateTimeFormat('en-GB', { 
      dateStyle: 'medium', 
      timeStyle: 'short' 
    }).format(date);
};

export const formatOnlyDate = (dateString) => {
    
  const date = new Date(dateString);

  return new Intl.DateTimeFormat('en-GB', { 
    dateStyle: 'medium'
  }).format(date);
};