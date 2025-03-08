import { useState, useEffect, useRef } from 'react';

export function useDebounce(value, delay = 500) {
  const [debouncedValue, setDebouncedValue] = useState(value);
  const timeoutRef = useRef();
  const forceUpdateRef = useRef(false);

  useEffect(() => {

    if (forceUpdateRef.current) {

      clearTimeout(timeoutRef.current);
      setDebouncedValue(value);
      forceUpdateRef.current = false;

      return;
    }

    // Update the url after timeout delay
    timeoutRef.current = setTimeout(() => {
      setDebouncedValue(value);

    }, delay);

    return () => clearTimeout(timeoutRef.current);
  }, [value, delay]);

  // Dont debounce update immediately
  const flush = () => {
    clearTimeout(timeoutRef.current);
    setDebouncedValue(value);
  };

  return [debouncedValue, { flush }];
}