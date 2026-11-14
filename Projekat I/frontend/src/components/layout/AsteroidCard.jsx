
import React, { useEffect, useState } from 'react';
import '../../styles/AsteroidCard.css';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export default function AsteroidCard({ asteroid, onClose }) {
    const [statistics, setStatistics] = useState({
        views: 0,
        searches: 0
    });

    const [statisticsLoading, setStatisticsLoading] = useState(true);
    const [statisticsError, setStatisticsError] = useState(null);

    useEffect(() => {
        if (!asteroid?.id) return;

        const controller = new AbortController();

        const fetchStatistics = async () => {
            setStatisticsLoading(true);
            setStatisticsError(null);

            try {
                const response = await fetch(
                    `${API_BASE_URL}/api/statistics/asteroids/${encodeURIComponent(asteroid.id)}`,
                    { signal: controller.signal }
                );

                if (!response.ok) {
                    throw new Error(
                        `Statistics request failed: ${response.status}`
                    );
                }

                const data = await response.json();

                setStatistics({
                    views: Number(data.views ?? 0),
                    searches: Number(data.searches ?? 0)
                });

            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Asteroid statistics error:', err);
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
    }, [asteroid?.id]);

    if (!asteroid) return null;

    const formatNumber = value =>
        Number(value).toLocaleString();

    const formatStatistic = value =>
        statisticsLoading ? '...' : formatNumber(value);

    return (
        <section
            className="asteroid-card-container"
            aria-label={`Details for ${asteroid.name}`}
        >
            <div className="asteroid-card-header">
                <div className="asteroid-card-title-group">
                    <div>
                        <p className="detail-eyebrow">
                            SELECTED OBJECT
                        </p>

                        <h3 className="asteroid-card-name">
                            {asteroid.name}
                        </h3>
                    </div>

                    {asteroid.hazardous && (
                        <span className="asteroid-card-pha-badge">
                            PHA · Hazardous
                        </span>
                    )}
                </div>

                {onClose && (
                    <button
                        type="button"
                        onClick={onClose}
                        className="asteroid-card-close-btn"
                        aria-label="Close asteroid details"
                    >
                        <span aria-hidden="true">×</span>
                    </button>
                )}
            </div>

            <div className="asteroid-card-body">

                <div className="asteroid-card-meta">
                    <span>
                        OBJECT ID <strong>{asteroid.id}</strong>
                    </span>

                    <span>
                        ORBIT CLASS <strong>{asteroid.orbitClass}</strong>
                    </span>
                </div>

                {/* Physical asteroid measurements */}
                <div className="asteroid-card-metrics">

                    <div className="metric-box">
                        <span className="metric-label">
                            Velocity
                        </span>

                        <span className="metric-value">
                            {Number(asteroid.speedKmS ?? 0).toFixed(1)}
                            {' '}
                            <small>km/s</small>
                        </span>
                    </div>

                    <div className="metric-box">
                        <span className="metric-label">
                            Diameter
                        </span>

                        <span className="metric-value">
                            {Number(asteroid.diameterKm ?? 0).toFixed(2)}
                            {' '}
                            <small>km</small>
                        </span>
                    </div>

                    <div className="metric-box">
                        <span className="metric-label">
                            Miss distance
                        </span>

                        <span className="metric-value">
                            {(Number(asteroid.missDistanceKm ?? 0) / 1000000).toFixed(2)}
                            {' '}
                            <small>M km</small>
                        </span>
                    </div>

                </div>

                {/* Individual Redis statistics */}
                <div className="asteroid-statistics">
                    <div className="asteroid-statistics-header">
                        <div>
                            <p className="detail-eyebrow">
                                REDIS ANALYTICS
                            </p>

                            <h4>
                                Object activity
                            </h4>
                        </div>

                        <span className="statistics-live-indicator">
                            <span className="status-dot" />
                            Statistics
                        </span>
                    </div>

                    <div className="asteroid-statistics-grid">

                        <div className="asteroid-stat-box">
                            <span className="asteroid-stat-icon">
                                ◉
                            </span>

                            <div>
                                <span className="asteroid-stat-label">
                                    Total views
                                </span>

                                <strong>
                                    {formatStatistic(statistics.views)}
                                </strong>
                            </div>
                        </div>

                        <div className="asteroid-stat-box">
                            <span className="asteroid-stat-icon asteroid-stat-icon--search">
                                ⌕
                            </span>

                            <div>
                                <span className="asteroid-stat-label">
                                    Total searches
                                </span>

                                <strong>
                                    {formatStatistic(statistics.searches)}
                                </strong>
                            </div>
                        </div>

                    </div>

                    {statisticsError && (
                        <p className="statistics-error">
                            Unable to load object statistics.
                        </p>
                    )}
                </div>

            </div>
        </section>
    );
}
