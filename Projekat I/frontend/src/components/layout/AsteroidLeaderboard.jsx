import React, { useEffect, useRef, useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidLeaderboard.css';

export const MOCK_ASTEROIDS = [
    { id: '2024-AB1', name: '2024 AB1', speedKmS: 42.5, diameterKm: 1.2, missDistanceKm: 1240000, hazardous: true, orbitClass: 'Apollo' },
    { id: '2023-XF7', name: '(2023 XF7)', speedKmS: 18.2, diameterKm: 3.8, missDistanceKm: 450000, hazardous: false, orbitClass: 'Aten' },
    { id: '99942-APOPHIS', name: '99942 Apophis', speedKmS: 30.7, diameterKm: 0.37, missDistanceKm: 31000, hazardous: true, orbitClass: 'Aten' },
    { id: '2021-AG', name: '2021 AG', speedKmS: 55.1, diameterKm: 0.85, missDistanceKm: 2150000, hazardous: false, orbitClass: 'Apollo' },
    { id: '2025-QZ', name: '2025 QZ', speedKmS: 12.4, diameterKm: 5.1, missDistanceKm: 7800000, hazardous: false, orbitClass: 'Amor' },
    { id: '2022-MK4', name: '2022 MK4', speedKmS: 38.9, diameterKm: 0.45, missDistanceKm: 620000, hazardous: true, orbitClass: 'Apollo' },
    { id: '2020-ND', name: '2020 ND', speedKmS: 22.3, diameterKm: 0.17, missDistanceKm: 5090000, hazardous: false, orbitClass: 'Apollo' },
    { id: '2026-HC', name: '2026 HC', speedKmS: 61.8, diameterKm: 2.4, missDistanceKm: 980000, hazardous: true, orbitClass: 'Aten' },
    { id: '2019-OD', name: '2019 OD', speedKmS: 25.4, diameterKm: 0.12, missDistanceKm: 358000, hazardous: false, orbitClass: 'Apollo' },
    { id: '2011-AG5', name: '2011 AG5', speedKmS: 15.0, diameterKm: 0.14, missDistanceKm: 1800000, hazardous: false, orbitClass: 'Apollo' }
];

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

const formatDistance = (km) => (km / 1000000).toFixed(2);

export default function AsteroidLeaderboard() {
    const [activeTab, setActiveTab] = useState('speed');
    const [selectedAsteroid, setSelectedAsteroid] = useState(null);
    const [openDropdownId, setOpenDropdownId] = useState(null);
    const dropdownRef = useRef(null);

    const sortedAsteroids = [...MOCK_ASTEROIDS].sort((a, b) => {
        if (activeTab === 'speed') return b.speedKmS - a.speedKmS;
        if (activeTab === 'diameter') return b.diameterKm - a.diameterKm;
        if (activeTab === 'distance') return a.missDistanceKm - b.missDistanceKm;
        return 0;
    });

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

    const handleFindSimilarBy = (targetAsteroid, criterion) => {
        const remaining = MOCK_ASTEROIDS.filter((asteroid) => asteroid.id !== targetAsteroid.id);
        const closestMatch = remaining.reduce((best, asteroid) => {
            const difference = Math.abs(asteroid[criterion === 'speed'
                ? 'speedKmS'
                : criterion === 'diameter'
                    ? 'diameterKm'
                    : 'missDistanceKm'] - targetAsteroid[criterion === 'speed'
                        ? 'speedKmS'
                        : criterion === 'diameter'
                            ? 'diameterKm'
                            : 'missDistanceKm']);
            const bestDifference = Math.abs(best[criterion === 'speed'
                ? 'speedKmS'
                : criterion === 'diameter'
                    ? 'diameterKm'
                    : 'missDistanceKm'] - targetAsteroid[criterion === 'speed'
                        ? 'speedKmS'
                        : criterion === 'diameter'
                            ? 'diameterKm'
                            : 'missDistanceKm']);
            return difference < bestDifference ? asteroid : best;
        }, remaining[0]);

        setSelectedAsteroid(closestMatch);
        setOpenDropdownId(null);
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
                    <span className="leaderboard-count">{sortedAsteroids.length} OBJECTS</span>
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
                                {sortedAsteroids.map((asteroid, index) => {
                                    const rankClass = index === 0 ? 'rank-gold' : index === 1 ? 'rank-silver' : index === 2 ? 'rank-bronze' : 'rank-normal';
                                    return (
                                        <tr key={asteroid.id} className="table-row">
                                            <td>
                                                <div className="cell-flex">
                                                    <span className={`rank-badge ${rankClass}`}>{String(index + 1).padStart(2, '0')}</span>
                                                    <div className="object-cell">
                                                        <div className="asteroid-name-row">
                                                            {asteroid.name}
                                                            {asteroid.hazardous && <span className="pha-badge">PHA</span>}
                                                        </div>
                                                        <div className="asteroid-id">ID: {asteroid.id}</div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td><span className="orbit-label">{asteroid.orbitClass}</span></td>
                                            <td className={`font-mono ${activeTab === 'speed' ? 'highlight-metric' : ''}`}>
                                                {asteroid.speedKmS.toFixed(1)} <span className="unit">km/s</span>
                                            </td>
                                            <td className={`font-mono ${activeTab === 'diameter' ? 'highlight-metric' : ''}`}>
                                                {asteroid.diameterKm.toFixed(2)} <span className="unit">km</span>
                                            </td>
                                            <td className={`font-mono ${activeTab === 'distance' ? 'highlight-metric' : ''}`}>
                                                {formatDistance(asteroid.missDistanceKm)} <span className="unit">M km</span>
                                            </td>
                                            <td className="text-right">{renderAction(asteroid)}</td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>

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
