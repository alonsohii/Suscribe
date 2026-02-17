import { useState, useEffect } from 'react';
import { AppBar, Toolbar, Typography, Box, Button } from '@mui/material';
import { useSearchParams } from 'react-router-dom';
import { SubscriptionForm } from './components/SubscriptionForm';
import { Dashboard } from './components/Dashboard';
import { AllSubscriptions } from './components/AllSubscriptions';
import { appStyles } from './styles/appStyles';

function App(): JSX.Element {
  const [searchParams] = useSearchParams();
  const [userId, setUserId] = useState<number | null>(null);
  const [view, setView] = useState<'form' | 'dashboard' | 'all'>('form');

  useEffect(() => {
    const userIdParam = searchParams.get('userId');
    if (userIdParam) {
      const id = parseInt(userIdParam);
      setUserId(id);
      setView('dashboard');
    }
  }, [searchParams]);

  const fromManage = searchParams.has('userId');

  return (
    <Box sx={appStyles.container}>
      <AppBar position="static">
        <Toolbar sx={appStyles.toolbar}>
          <Typography variant="h6" sx={appStyles.title}>Sistema de Suscripciones Premium</Typography>
          <Box sx={appStyles.buttonGroup}>
            <Button color="inherit" onClick={() => { setUserId(null); setView('form'); }}>Nueva Suscripción</Button>
            <Button color="inherit" onClick={() => setView('all')}>Ver Todas</Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Box component="main" sx={appStyles.main}>
        {view === 'all' ? (
          <AllSubscriptions />
        ) : !userId ? (
          <SubscriptionForm onSuccess={(id) => { setUserId(id); setView('dashboard'); }} />
        ) : (
          <Dashboard userId={userId as number} fromManage={fromManage} />
        )}
      </Box>

      <Box component="footer" sx={appStyles.footer}>
        <Typography variant="body2" color="text.secondary">
          Sistema de Suscripciones
        </Typography>
      </Box>
    </Box>
  );
}

export default App;
