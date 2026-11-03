import React from 'react';
import '../../styles/AsteroidCard.css';

export default function AsteroidCard({ asteroid, onClose }) {
    if (!asteroid) return null;

    return (
        <div className="asteroid-card-container">
            <div className="asteroid-card-header">
                <div className="asteroid-card-title-group">
                    <h3 className="asteroid-card-name">{asteroid.name}</h3>
                    {asteroid.hazardous && (
                        <span className="asteroid-card-pha-badge">PHA (Hazardous)</span>
                    )}
                </div>
                {onClose && (
                    <button onClick={onClose} className="asteroid-card-close-btn" aria-label="Close card">
                        &times;
                    </button>
                )}
            </div>

            <div className="asteroid-card-body">
                <div className="asteroid-card-meta">
                    <span className="asteroid-card-id">ID: {asteroid.id}</span>
                    <span className="asteroid-card-orbit">Orbit Class: {asteroid.orbitClass}</span>
                </div>

                <div className="asteroid-card-metrics">
                    <div className="metric-box">
                        <span className="metric-label">Velocity</span>
                        <span className="metric-value">
                            {asteroid.speedKmS.toFixed(1)} <small>km/s</small>
                        </span>
                    </div>

                    <div className="metric-box">
                        <span className="metric-label">Diameter</span>
                        <span className="metric-value">
                            {asteroid.diameterKm.toFixed(2)} <small>km</small>
                        </span>
                    </div>

                    <div className="metric-box">
                        <span className="metric-label">Miss Distance</span>
                        <span className="metric-value">
                            {(asteroid.missDistanceKm / 1000000).toFixed(2)} <small>M km</small>
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}