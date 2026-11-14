import React, { useEffect, useRef, useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidLeaderboard.css';

export const MOCK_ASTEROIDS = {};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

const ENDPOINTS = {
    speed: 'fastest',
    diameter: 'largest',
    distance: 'closest'
};

const TABS = [
    { id: 'speed', label: 'Top speed', icon: '↗' },
    { id: 'diameter', label: 'Largest diameter', icon: '◉' },
    { id: 'distance', label: 'Closest approach', icon: '◎' }
];

const CRITERIA = [
    { id: 'speed', label: 'By speed' },
    { id: 'diameter', label: 'By diameter' },
    { id: 'distance', label: 'By distance' }
];

// za kriterijum sličnosti koji se šalje na back za pretragu
const SIMILARITY_CRITERIA = {
    speed: 'velocity',
    diameter: 'diameter',
    distance: 'missdistance'
};

const formatDistance = (km) => (km / 1000000).toFixed(2);

export default function AsteroidLeaderboard() {
    const [activeTab, setActiveTab] = useState('speed');
    const [selectedAsteroid, setSelectedAsteroid] = useState(null);
    const [openDropdownId, setOpenDropdownId] = useState(null);
    const [asteroids, setAsteroids] = useState([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // za sličnost asteroida
    const [similarAsteroids, setSimilarAsteroids] = useState([]);
    const [similarLoading, setSimilarLoading] = useState(false);
    const [similarError, setSimilarError] = useState(null);
    const [similarTitle, setSimilarTitle] = useState('');

    const dropdownRef = useRef(null);

    useEffect(() => {
        const controller = new AbortController();

        const fetchLeaderboard = async () => {
            setLoading(true);
            setError(null);
            setSelectedAsteroid(null);
            setOpenDropdownId(null);

            try {
                const endpoint = ENDPOINTS[activeTab];

                const response = await fetch(
                    `${API_BASE_URL}/api/leaderboard/${endpoint}?count=10`,
                    { signal: controller.signal }
                );

                if (!response.ok) {
                    throw new Error(
                        `Leaderboard request failed: ${response.status}`
                    );
                }

                const data = await response.json();
                setAsteroids(data);
            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Leaderboard error:', err);
                    setError(err.message || 'Unable to load leaderboard.');
                }
            } finally {
                if (!controller.signal.aborted) {
                    setLoading(false);
                }
            }
        };

        fetchLeaderboard();

        return () => controller.abort();
    }, [activeTab]);
    // Redis sadrži već sortirane asteroide
    const sortedAsteroids = asteroids;

    // zatvaranje dropdown menija za slične asteroide
    useEffect(() => {
        const handlePointerDown = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setOpenDropdownId(null);
            }
        };
        const handleKeyDown = (event) => {
            if (event.key === 'Escape') setOpenDropdownId(null);
        };

        document.addEventListener('pointerdown', handlePointerDown);
        document.addEventListener('keydown', handleKeyDown);
        return () => {
            document.removeEventListener('pointerdown', handlePointerDown);
            document.removeEventListener('keydown', handleKeyDown);
        };
    }, []);

    const handleFindSimilarBy = async (targetAsteroid, criterion) => {
        setOpenDropdownId(null);
        setSelectedAsteroid(null);

        setSimilarAsteroids([]);
        setSimilarError(null);
        setSimilarLoading(true);

        const backendCriterion = SIMILARITY_CRITERIA[criterion];

        const labels = {
            speed: 'Speed similarity',
            diameter: 'Diameter similarity',
            distance: 'Miss-distance similarity'
        };

        setSimilarTitle(
            `${labels[criterion]} — ${targetAsteroid.name}`
        );

        if (!backendCriterion) {
            setSimilarError(`Unsupported similarity criterion: ${criterion}`);
            setSimilarLoading(false);
            return;
        }

        try {
            const response = await fetch(
                `${API_BASE_URL}/Similarity/${encodeURIComponent(targetAsteroid.id)}/${backendCriterion}`
            );

            if (!response.ok) {
                const errorBody = await response.text();

                throw new Error(
                    `Similarity request failed (${response.status}): ${errorBody}`
                );
            }

            const data = await response.json();

            if (!Array.isArray(data)) {
                throw new Error('Invalid similarity response from server.');
            }

            setSimilarAsteroids(data);

        } catch (err) {
            console.error('Similarity request error:', err);

            setSimilarError(
                err.message || 'Unable to load similar asteroids.'
            );
        } finally {
            setSimilarLoading(false);
        }
    };

    const renderAction = (asteroid) => (
        <div className="dropdown-wrapper" ref={openDropdownId === asteroid.id ? dropdownRef : null}>
            <button
                type="button"
                className="action-btn"
                aria-haspopup="menu"
                aria-expanded={openDropdownId === asteroid.id}
                aria-controls={`similar-menu-${asteroid.id}`}
                onClick={() => setOpenDropdownId((current) => current === asteroid.id ? null : asteroid.id)}
            >
                Similar <span aria-hidden="true">{openDropdownId === asteroid.id ? '▴' : '▾'}</span>
            </button>
            {openDropdownId === asteroid.id && (
                <div className="similar-dropdown-menu" id={`similar-menu-${asteroid.id}`} role="menu">
                    {CRITERIA.map((criterion) => (
                        <button
                            type="button"
                            className="dropdown-item"
                            role="menuitem"
                            key={criterion.id}
                            onClick={() => handleFindSimilarBy(asteroid, criterion.id)}
                        >
                            {criterion.label}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );

    const getAsteroidMetrics = (asteroid) => {
        const approaches = asteroid.approaches ?? [];

        const velocities = approaches
            .map(a => Number(a.relativeVelocityKps))
            .filter(Number.isFinite);

        const distances = approaches
            .map(a => Number(a.missDistance))
            .filter(Number.isFinite);

        return {
            velocity: velocities.length
                ? Math.max(...velocities)
                : null,

            diameter: asteroid.diameterMaxKm ?? null,

            missDistance: distances.length
                ? Math.min(...distances)
                : null
        };
    };

    const formatSimilarity = (value) => {
        if (value == null || !Number.isFinite(Number(value))) {
            return 'N/A';
        }

        // Assumes the backend returns similarity in the 0–1 range.
        return `${(Number(value) * 100).toFixed(1)}%`;
    };

    const renderSimilarAsteroid = (result, index) => {
        const asteroid = result.asteroid;

        if (!asteroid) return null;

        const metrics = getAsteroidMetrics(asteroid);

        return (
            <article
                className="similar-result-card"
                key={asteroid.id}
            >
                <div className="similar-result-header">
                    <div className="similar-result-identity">
                        <span className="similar-result-rank">
                            {String(index + 1).padStart(2, '0')}
                        </span>

                        <div className="similar-result-name-group">
                            <h3>{asteroid.name}</h3>

                            <span className="similar-result-id">
                                ID: {asteroid.id}
                            </span>
                        </div>
                    </div>

                    {asteroid.isHazardous && (
                        <span className="pha-badge">PHA</span>
                    )}
                </div>

                <div className="similarity-score-row">
                    <div>
                        <span className="similarity-score-label">
                            OVERALL SIMILARITY
                        </span>

                        <strong className="similarity-score">
                            {formatSimilarity(result.overallSimilarity)}
                        </strong>
                    </div>

                    <div>
                        <span className="similarity-score-label">
                            SELECTED CRITERION
                        </span>

                        <strong className="similarity-criterion-score">
                            {formatSimilarity(result.criterionSimilarity)}
                        </strong>
                    </div>
                </div>

                <div className="similar-result-metrics">
                    <div className="similar-metric">
                        <span>VELOCITY</span>
                        <strong>
                            {metrics.velocity != null
                                ? `${metrics.velocity.toFixed(2)} km/s`
                                : 'N/A'}
                        </strong>
                    </div>

                    <div className="similar-metric">
                        <span>DIAMETER</span>
                        <strong>
                            {metrics.diameter != null
                                ? `${Number(metrics.diameter).toFixed(2)} km`
                                : 'N/A'}
                        </strong>
                    </div>

                    <div className="similar-metric">
                        <span>MISS DISTANCE</span>
                        <strong>
                            {metrics.missDistance != null
                                ? `${(metrics.missDistance / 1_000_000).toFixed(2)} M km`
                                : 'N/A'}
                        </strong>
                    </div>
                </div>
            </article>
        );
    };

    return (
        <section className="asteroid-container" aria-labelledby="leaderboard-title">
            <div className="starfield-overlay" aria-hidden="true" />
            <div className="asteroid-content">
                <header className="asteroid-header">
                    <div>
                        <div className="telemetry-badge">
                            <span className="pulsing-dot" aria-hidden="true" />
                            CATALOG OVERVIEW
                        </div>
                        <h2 className="asteroid-title" id="leaderboard-title">Near-Earth object leaderboard</h2>
                    </div>
                    <p className="asteroid-subtitle">
                        Compare orbital measurements for the ten objects in this demonstration catalog.
                    </p>
                </header>

                <div className="leaderboard-toolbar">
                    <div className="tab-group" role="group" aria-label="Sort asteroids">
                        {TABS.map((tab) => (
                            <button
                                type="button"
                                key={tab.id}
                                onClick={() => {
                                    setActiveTab(tab.id);
                                    setOpenDropdownId(null);
                                }}
                                className={`tab-btn ${activeTab === tab.id ? 'active' : ''}`}
                                aria-pressed={activeTab === tab.id}
                            >
                                <span className="tab-icon" aria-hidden="true">{tab.icon}</span>
                                {tab.label}
                            </button>
                        ))}
                    </div>
                    <span className="leaderboard-count">
                        {loading ? 'LOADING...' : `${sortedAsteroids.length} OBJECTS`}
                    </span>
                </div>

                <div className="table-wrapper">
                    <div className="table-scroll">
                        <table className="asteroid-table">
                            <thead>
                                <tr>
                                    <th scope="col">Rank &amp; object</th>
                                    <th scope="col">Orbit class</th>
                                    <th scope="col">Velocity</th>
                                    <th scope="col">Diameter</th>
                                    <th scope="col">Miss distance</th>
                                    <th scope="col" className="text-right">Action</th>
                                </tr>
                            </thead>

                            <tbody>
                                {loading ? (
                                    <tr>
                                        <td colSpan="6" className="table-message">
                                            Loading asteroid data...
                                        </td>
                                    </tr>
                                ) : error ? (
                                    <tr>
                                        <td colSpan="6" className="table-message error">
                                            {error}
                                        </td>
                                    </tr>
                                ) : sortedAsteroids.length === 0 ? (
                                    <tr>
                                        <td colSpan="6" className="table-message">
                                            No asteroids found.
                                        </td>
                                    </tr>
                                ) : (
                                    sortedAsteroids.map((asteroid, index) => {
                                        const rankClass =
                                            index === 0 ? 'rank-gold' :
                                                index === 1 ? 'rank-silver' :
                                                    index === 2 ? 'rank-bronze' :
                                                        'rank-normal';

                                        return (
                                            <tr key={asteroid.id} className="table-row">
                                                <td>
                                                    <div className="cell-flex">
                                                        <span className={`rank-badge ${rankClass}`}>
                                                            {String(index + 1).padStart(2, '0')}
                                                        </span>

                                                        <div className="object-cell">
                                                            <div className="asteroid-name-row">
                                                                {asteroid.name}

                                                                {asteroid.hazardous && (
                                                                    <span className="pha-badge">
                                                                        PHA
                                                                    </span>
                                                                )}
                                                            </div>

                                                            <div className="asteroid-id">
                                                                ID: {asteroid.id}
                                                            </div>
                                                        </div>
                                                    </div>
                                                </td>

                                                <td>
                                                    <span className="orbit-label">
                                                        {asteroid.orbitClass}
                                                    </span>
                                                </td>

                                                <td className={`font-mono ${activeTab === 'speed' ? 'highlight-metric' : ''
                                                    }`}>
                                                    {asteroid.speedKmS.toFixed(1)}
                                                    <span className="unit"> km/s</span>
                                                </td>

                                                <td className={`font-mono ${activeTab === 'diameter' ? 'highlight-metric' : ''
                                                    }`}>
                                                    {asteroid.diameterKm.toFixed(2)}
                                                    <span className="unit"> km</span>
                                                </td>

                                                <td className={`font-mono ${activeTab === 'distance' ? 'highlight-metric' : ''
                                                    }`}>
                                                    {formatDistance(asteroid.missDistanceKm)}
                                                    <span className="unit"> M km</span>
                                                </td>

                                                <td className="text-right">
                                                    {renderAction(asteroid)}
                                                </td>
                                            </tr>
                                        );
                                    })
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Similarity results */}
                {(similarLoading || similarError || similarTitle || similarAsteroids.length > 0) && (
                    <section
                        className="similar-results-section"
                        aria-live="polite"
                        aria-labelledby="similar-results-title"
                    >
                        <header className="similar-results-header">
                            <div>
                                <div className="telemetry-badge">
                                    <span className="pulsing-dot" aria-hidden="true" />
                                    SIMILARITY ANALYSIS
                                </div>

                                <h2 id="similar-results-title">
                                    {similarTitle || 'Similar asteroids'}
                                </h2>

                                <p>
                                    Asteroids returned by the similarity analysis.
                                </p>
                            </div>

                            <div className="similar-results-actions">
                                {!similarLoading && !similarError && (
                                    <span className="leaderboard-count">
                                        {similarAsteroids.length} MATCHES
                                    </span>
                                )}

                                <button
                                    type="button"
                                    className="similar-clear-btn"
                                    onClick={() => {
                                        setSimilarAsteroids([]);
                                        setSimilarTitle('');
                                        setSimilarError(null);
                                    }}
                                >
                                    Clear
                                </button>
                            </div>
                        </header>

                        {similarLoading ? (
                            <div className="similar-results-message">
                                Calculating asteroid similarities...
                            </div>
                        ) : similarError ? (
                            <div className="similar-results-message error">
                                {similarError}
                            </div>
                        ) : similarAsteroids.length === 0 ? (
                            <div className="similar-results-message">
                                No similar asteroids found for this criterion.
                            </div>
                        ) : (
                            <div className="similar-results-grid">
                                {similarAsteroids.map(renderSimilarAsteroid)}
                            </div>
                        )}
                    </section>
                )}

                {selectedAsteroid && (
                    <AsteroidCard
                        asteroid={selectedAsteroid}
                        onClose={() => setSelectedAsteroid(null)}
                    />
                )}
            </div>
        </section>
    );
}
