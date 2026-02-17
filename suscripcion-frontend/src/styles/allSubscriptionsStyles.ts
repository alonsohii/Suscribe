export const allSubscriptionsStyles = {
  loadingBox: { display: 'flex', justifyContent: 'center', mt: 4 },
  container: { mt: 4, px: { xs: 1, sm: 2 } },
  title: { mb: 3, fontSize: { xs: '1.5rem', sm: '2rem' } },
  tableContainer: { overflowX: 'auto' },
  table: { minWidth: { xs: 300, sm: 650 } },
  headerCell: { fontSize: { xs: '0.75rem', sm: '0.875rem' } },
  headerCellHidden: { display: { xs: 'none', md: 'table-cell' }, fontSize: { xs: '0.75rem', sm: '0.875rem' } },
  bodyCell: { fontSize: { xs: '0.75rem', sm: '0.875rem' }, py: { xs: 1, sm: 2 } },
  emailCell: { 
    fontSize: { xs: '0.75rem', sm: '0.875rem' }, 
    py: { xs: 1, sm: 2 }, 
    wordBreak: 'break-word', 
    maxWidth: { xs: '150px', sm: 'none' } 
  },
  statusCell: { py: { xs: 1, sm: 2 } },
  chip: { fontSize: { xs: '0.65rem', sm: '0.8125rem' } },
  dateCell: { 
    display: { xs: 'none', md: 'table-cell' }, 
    fontSize: { xs: '0.75rem', sm: '0.875rem' }, 
    py: { xs: 1, sm: 2 } 
  },
  actionCell: { py: { xs: 1, sm: 2 } },
  button: { 
    fontSize: { xs: '0.65rem', sm: '0.875rem' },
    px: { xs: 1, sm: 2 },
    py: { xs: 0.5, sm: 1 }
  }
};
