import React from 'react';
import AsteroidDashboardMetrics from './components/layout/AsteroidDashboardMetrics';
import AsteroidLeaderboard from './components/layout/AsteroidLeaderboard';

export default function App() {
  return (
    <div style={{ backgroundColor: '#070913', minHeight: '100vh', padding: '2rem' }}>
      <main style={{ maxWidth: '64rem', margin: '0 auto' }}>

        <AsteroidDashboardMetrics
          hazardousCount={14}
          apiCalls={342}
          totalTracked={1250}
        />

        <AsteroidLeaderboard />

      </main>
    </div>
  );
}