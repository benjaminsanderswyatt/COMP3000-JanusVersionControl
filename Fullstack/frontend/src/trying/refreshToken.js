import { useEffect } from 'react';
import { checkAndRefreshToken } from '../api/fetchUsers';


// Every interval check the token and refresh
const useTokenRefresh = () => {
  useEffect(() => {
    const interval = setInterval(() => {
      checkAndRefreshToken();
    }, 5 * 60 * 1000); // Check every 5 minutes

    return () => clearInterval(interval); // Cleanup
  }, []);
};


export default useTokenRefresh;
