import React, { useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidLeaderboard.css'

const MOCK_ASTEROIDS = [
    {
        id: "2024-AB1",
        name: "2024 AB1",
        speedKmS: 42.5,
        diameterKm: 1.2,
        missDistanceKm: 1240000,
        hazardous: true,
        orbitClass: "Apollo"
    },
    {
        id: "2023-XF7",
        name: "(2023 XF7)",
        speedKmS: 18.2,
        diameterKm: 3.8,
        missDistanceKm: 450000,
        hazardous: false,
        orbitClass: "Aten"
    },
    {
        id: "99942-APOPHIS",
        name: "99942 Apophis",
        speedKmS: 30.7,
        diameterKm: 0.37,
        missDistanceKm: 31000,
        hazardous: true,
        orbitClass: "Aten"
    },
    {
        id: "2021-AG",
        name: "2021 AG",
        speedKmS: 55.1,
        diameterKm: 0.85,
        missDistanceKm: 2150000,
        hazardous: false,
        orbitClass: "Apollo"
    },
    {
        id: "2025-QZ",
        name: "2025 QZ",
        speedKmS: 12.4,
        diameterKm: 5.1,
        missDistanceKm: 7800000,
        hazardous: false,
        orbitClass: "Amor"
    },
    {
        id: "2022-MK4",
        name: "2022 MK4",
        speedKmS: 38.9,
        diameterKm: 0.45,
        missDistanceKm: 620000,
        hazardous: true,
        orbitClass: "Apollo"
    },
    {
        id: "2020-ND",
        name: "2020 ND",
        speedKmS: 22.3,
        diameterKm: 0.17,
        missDistanceKm: 5090000,
        hazardous: false,
        orbitClass: "Apollo"
    },
    {
        id: "2026-HC",
        name: "2026 HC",
        speedKmS: 61.8,
        diameterKm: 2.4,
        missDistanceKm: 980000,
        hazardous: true,
        orbitClass: "Aten"
    },
    {
        id: "2019-OD",
        name: "2019 OD",
        speedKmS: 25.4,
        diameterKm: 0.12,
        missDistanceKm: 358000,
        hazardous: false,
        orbitClass: "Apollo"
    },
    {
        id: "2011-AG5",
        name: "2011 AG5",
        speedKmS: 15.0,
        diameterKm: 0.14,
        missDistanceKm: 1800000,
        hazardous: false,
        orbitClass: "Apollo"
    }
];

export default function AsteroidLeaderboard() {
    const [activeTab, setActiveTab] = useState('speed');
    const [selectedAsteroid, setSelectedAsteroid] = useState(null);

    const sortedAsteroids = [...MOCK_ASTEROIDS].sort((a, b) => {
        if (activeTab === 'speed') return b.speedKmS - a.speedKmS;
        if (activeTab === 'diameter') return b.diameterKm - a.diameterKm;
        if (activeTab === 'distance') return a.missDistanceKm - b.missDistanceKm;
        return 0;
    });

    const handleShowSimilar = (asteroid) => {
        setSelectedAsteroid(asteroid);
    };

    return (
        <div className="asteroid-container">
            <div className="starfield-overlay" />

            <div className="asteroid-content">

                {/* Header Section */}
                <header className="asteroid-header">
                    <div>
                        <div className="telemetry-badge">
                            <span className="pulsing-dot"></span>
                            NASA NeoWS Telemetry Feed
                        </div>
                        <h1 className="asteroid-title">
                            Near-Earth Object Leaderboard
                        </h1>
                    </div>
                    <p className="asteroid-subtitle">
                        Real-time telemetry and orbital metrics for top 10 tracked cosmic bodies.
                    </p>
                </header>

                {/* Navigation Tabs */}
                <div className="tab-group">
                    {[
                        { id: 'speed', label: 'Top Speed', icon: '⚡' },
                        { id: 'diameter', label: 'Largest Diameter', icon: '🪐' },
                        { id: 'distance', label: 'Closest Approach', icon: '🎯' },
                    ].map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`tab-btn ${activeTab === tab.id ? 'active' : ''}`}
                        >
                            <span>{tab.icon}</span>
                            {tab.label}
                        </button>
                    ))}
                </div>

                {/* Leaderboard Table Wrapper */}
                <div className="table-wrapper">
                    <div className="table-scroll">
                        <table className="asteroid-table">
                            <thead>
                                <tr>
                                    <th>Rank & Object</th>
                                    <th>Orbit Class</th>
                                    <th>Velocity</th>
                                    <th>Diameter</th>
                                    <th>Miss Distance</th>
                                    <th className="text-right">Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {sortedAsteroids.map((asteroid, index) => {
                                    const rankClass =
                                        index === 0 ? 'rank-gold' :
                                            index === 1 ? 'rank-silver' :
                                                index === 2 ? 'rank-bronze' : 'rank-normal';

                                    return (
                                        <tr key={asteroid.id} className="table-row">
                                            {/* Rank & Name */}
                                            <td className="cell-flex">
                                                <span className={`rank-badge ${rankClass}`}>
                                                    0{index + 1}
                                                </span>
                                                <div>
                                                    <div className="asteroid-name-row">
                                                        {asteroid.name}
                                                        {asteroid.hazardous && (
                                                            <span className="pha-badge">PHA</span>
                                                        )}
                                                    </div>
                                                    <div className="asteroid-id">ID: {asteroid.id}</div>
                                                </div>
                                            </td>

                                            {/* Orbit Class */}
                                            <td className="font-mono text-slate">{asteroid.orbitClass}</td>

                                            {/* Speed */}
                                            <td className={`font-mono ${activeTab === 'speed' ? 'highlight-metric' : 'text-slate'}`}>
                                                {asteroid.speedKmS.toFixed(1)} <span className="unit">km/s</span>
                                            </td>

                                            {/* Diameter */}
                                            <td className={`font-mono ${activeTab === 'diameter' ? 'highlight-metric' : 'text-slate'}`}>
                                                {asteroid.diameterKm.toFixed(2)} <span className="unit">km</span>
                                            </td>

                                            {/* Distance */}
                                            <td className={`font-mono ${activeTab === 'distance' ? 'highlight-metric' : 'text-slate'}`}>
                                                {(asteroid.missDistanceKm / 1000000).toFixed(2)} <span className="unit">M km</span>
                                            </td>

                                            {/* Action */}
                                            <td className="text-right">
                                                <button
                                                    onClick={() => handleShowSimilar(asteroid)}
                                                    className="action-btn"
                                                >
                                                    Show Similar
                                                </button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Dismissible Preview Banner */}
                {selectedAsteroid && (
                    <AsteroidCard
                        asteroid={selectedAsteroid}
                        onClose={() => setSelectedAsteroid(null)}
                    />
                )}

            </div>
        </div>
    );
}