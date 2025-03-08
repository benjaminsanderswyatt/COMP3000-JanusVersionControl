import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { useDebounce } from '../../helpers/Debounce';

export const useSearch = ( delay = 500 ) => {
  const [searchParams] = useSearchParams();
  const initialQuery = searchParams.get("search") || '';
  const [searchValue, setSearchValue] = useState(initialQuery);
  const debouncedSearchValue = useDebounce(searchValue, delay);
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(searchParams);

    if (debouncedSearchValue) {
      params.set("search", debouncedSearchValue);
    } else {
      params.delete("search");
    }

    navigate(`?${params.toString()}`, { replace: true }); // Replace handles browser history

  }, [debouncedSearchValue, navigate, searchParams, "search"]);

  return [searchValue, setSearchValue, debouncedSearchValue];
};
