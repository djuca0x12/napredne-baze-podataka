
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

                // Adapt backend data to the existing frontend format.
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
                    hazardousCount={
                        asteroids.filter(a => a.hazardous).length
                    }
                    apiCalls={342}
                    totalTracked={asteroids.length}
                />

                {error && (
                    <p className="error-message">
                        Unable to load asteroid catalog: {error}
                    </p>
                )}

                <AsteroidSearch asteroids={asteroids} />

                <AsteroidLeaderboard />

            </main>
        </div>
    );
}
