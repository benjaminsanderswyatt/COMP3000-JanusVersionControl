import React from 'react';
import styles from '../styles/ItemList.module.css';

const ItemList = ({ items }) => {
  return (
    <ul className={styles.list}>
      {items.map((item) => (
        <li key={item.id} className={styles.listItem}>
          {item.name}
        </li>
      ))}
    </ul>
  );
};

export default ItemList;
