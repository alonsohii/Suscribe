import { useEffect, useState } from 'react';
import { Container, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip, CircularProgress, Box, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { subscriptionService } from '../services/api';
import type { Subscription } from '../types/api';
import { allSubscriptionsStyles as styles } from '../styles/allSubscriptionsStyles';

export function AllSubscriptions() {
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    subscriptionService.getAllSubscriptions()
      .then(setSubscriptions)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Box sx={styles.loadingBox}><CircularProgress /></Box>;

  return (
    <Container maxWidth="lg" sx={styles.container}>
      <Typography variant="h4" sx={styles.title}>Todas las Suscripciones</Typography>
      <TableContainer component={Paper} sx={styles.tableContainer}>
        <Table sx={styles.table}>
          <TableHead>
            <TableRow>
              <TableCell sx={styles.headerCell}>ID</TableCell>
              <TableCell sx={styles.headerCell}>Email / Usuario</TableCell>
              <TableCell sx={styles.headerCell}>Estado</TableCell>
              <TableCell sx={styles.headerCellHidden}>Fecha</TableCell>
              <TableCell align="center" sx={styles.headerCell}>Acciones</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {subscriptions.map(sub => (
              <TableRow key={sub.id}>
                <TableCell sx={styles.bodyCell}>{sub.id}</TableCell>
                <TableCell sx={styles.emailCell}>
                  {sub.userEmail || `Usuario ${sub.userId}`}
                </TableCell>
                <TableCell sx={styles.statusCell}>
                  <Chip 
                    label={sub.status} 
                    color={sub.status === 'Active' ? 'success' : 'default'} 
                    size="small"
                    sx={styles.chip}
                  />
                </TableCell>
                <TableCell sx={styles.dateCell}>
                  {new Date(sub.createdAt).toLocaleString()}
                </TableCell>
                <TableCell align="center" sx={styles.actionCell}>
                  <Button 
                    variant="outlined" 
                    size="small"
                    onClick={() => navigate(`/?userId=${sub.userId}`)}
                    sx={styles.button}
                  >
                    Gestionar
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Container>
  );
}
