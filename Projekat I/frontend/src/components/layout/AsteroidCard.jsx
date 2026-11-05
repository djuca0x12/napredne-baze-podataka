import React from 'react';
import '../../styles/AsteroidCard.css';

export default function AsteroidCard({ asteroid, onClose }) {
    if (!asteroid) return null;

    return (
        <section className="asteroid-card-container" aria-label={`Details for ${asteroid.name}`}>
            <div className="asteroid-card-header">
                <div className="asteroid-card-title-group">
                    <div>
                        <p className="detail-eyebrow">SELECTED OBJECT</p>
                        <h3 className="asteroid-card-name">{asteroid.name}</h3>
                    </div>
                    {asteroid.hazardous && (
                        <span className="asteroid-card-pha-badge">PHA · Hazardous</span>
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
                    <span>OBJECT ID <strong>{asteroid.id}</strong></span>
                    <span>ORBIT CLASS <strong>{asteroid.orbitClass}</strong></span>
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
                        <span className="metric-label">Miss distance</span>
                        <span className="metric-value">
                            {(asteroid.missDistanceKm / 1000000).toFixed(2)} <small>M km</small>
                        </span>
                    </div>
                </div>
            </div>
        </section>
    );
}
