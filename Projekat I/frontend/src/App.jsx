import React from 'react';
import AsteroidDashboardMetrics from './components/layout/AsteroidDashboardMetrics';
import AsteroidLeaderboard, { MOCK_ASTEROIDS } from './components/layout/AsteroidLeaderboard';
import AsteroidSearch from './components/layout/AsteroidSearch';
import './styles/App.css';

export default function App() {
    return (
        <div className="app-layout">
            <main className="app-content">
                <header className="app-heading">
                    <div className="app-eyebrow">
                        <span className="pulsing-dot" aria-hidden="true" />
                        SPACE OBJECT MONITOR
                    </div>
                    <h1 className="app-title">Asteroid Observatory</h1>
                    <p className="app-description">
                        A focused overview of near-Earth objects, orbital measurements, and catalog activity.
                    </p>
                </header>

                <AsteroidDashboardMetrics
                    hazardousCount={14}
                    apiCalls={342}
                    totalTracked={1250}
                />

                <AsteroidSearch asteroids={MOCK_ASTEROIDS} />
                <AsteroidLeaderboard />
            </main>
        </div>
    );
}
