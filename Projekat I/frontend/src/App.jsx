
import React, { useEffect, useState } from 'react';

import AsteroidDashboardMetrics from './components/layout/AsteroidDashboardMetrics';
import AsteroidLeaderboard from './components/layout/AsteroidLeaderboard';
import AsteroidSearch from './components/layout/AsteroidSearch';

import './styles/App.css';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export default function App() {
    const [asteroids, setAsteroids] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const [statistics, setStatistics] = useState({
        totalViews: 0,
        totalSearches: 0
    });

    const [statisticsLoading, setStatisticsLoading] = useState(true);
    const [statisticsError, setStatisticsError] = useState(null);

    // učitava asteroide sa backend-a
    useEffect(() => {
        const controller = new AbortController();

        const fetchAsteroids = async () => {
            try {
                const response = await fetch(
                    `${API_BASE_URL}/Asteroid`,
                    { signal: controller.signal }
                );

                if (!response.ok) {
                    throw new Error(
                        `Failed to fetch asteroids: ${response.status}`
                    );
                }

                const data = await response.json();

                // prilagođava podatke za format prikaza na frontu
                const formattedAsteroids = data.map((asteroid) => {
                    const approaches = asteroid.approaches ?? [];

                    const speeds = approaches
                        .map(a => a.relativeVelocityKps)
                        .filter(Number.isFinite);

                    const distances = approaches
                        .map(a => a.missDistance)
                        .filter(Number.isFinite);

                    return {
                        ...asteroid,

                        id: asteroid.id,
                        name: asteroid.name,

                        speedKmS: speeds.length
                            ? Math.max(...speeds)
                            : 0,

                        diameterKm: asteroid.diameterMaxKm ?? 0,

                        missDistanceKm: distances.length
                            ? Math.min(...distances)
                            : 0,

                        hazardous: asteroid.isHazardous,

                        orbitClass: 'Unknown'
                    };
                });

                setAsteroids(formattedAsteroids);

            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Asteroid fetch error:', err);
                    setError(err.message);
                }
            } finally {
                if (!controller.signal.aborted) {
                    setLoading(false);
                }
            }
        };

        fetchAsteroids();

        return () => controller.abort();
    }, []);

    // Redis global statistics
    useEffect(() => {
        const controller = new AbortController();

        const fetchStatistics = async () => {
            try {
                const response = await fetch(
                    `${API_BASE_URL}/api/statistics`,
                    { signal: controller.signal }
                );

                if (!response.ok) {
                    throw new Error(
                        `Failed to fetch statistics: ${response.status}`
                    );
                }

                const data = await response.json();

                setStatistics({
                    totalViews: Number(data.totalViews ?? 0),
                    totalSearches: Number(data.totalSearches ?? 0)
                });

                setStatisticsError(null);

            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Statistics fetch error:', err);
                    setStatisticsError(err.message);
                }
            } finally {
                if (!controller.signal.aborted) {
                    setStatisticsLoading(false);
                }
            }
        };

        fetchStatistics();

        return () => controller.abort();
    }, []);

    const hazardousCount = asteroids.filter(
        asteroid => asteroid.hazardous
    ).length;

    return (
        <div className="app-layout">
            <main className="app-content">

                <header className="app-heading">
                    <div className="app-eyebrow">
                        <span
                            className="pulsing-dot"
                            aria-hidden="true"
                        />
                        SPACE OBJECT MONITOR
                    </div>

                    <h1 className="app-title">
                        Asteroid Observatory
                    </h1>

                    <p className="app-description">
                        A focused overview of near-Earth objects,
                        orbital measurements, and catalog activity.
                    </p>
                </header>

                <AsteroidDashboardMetrics
                    hazardousCount={hazardousCount}
                    apiCalls={342}
                    totalTracked={asteroids.length}
                    totalViews={statistics.totalViews}
                    totalSearches={statistics.totalSearches}
                    statisticsLoading={statisticsLoading}
                />

                {error && (
                    <p className="error-message">
                        Unable to load asteroid catalog: {error}
                    </p>
                )}

                {statisticsError && (
                    <p className="error-message">
                        Unable to load usage statistics: {statisticsError}
                    </p>
                )}

                <AsteroidSearch />

                <AsteroidLeaderboard />

            </main>
        </div>
    );
}