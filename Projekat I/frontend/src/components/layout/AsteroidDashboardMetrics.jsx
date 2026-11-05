import React from 'react';
import '../../styles/AsteroidDashboardMetrics.css';

export default function AsteroidDashboardMetrics({
    hazardousCount = 14,
    apiCalls = 342,
    totalTracked = 1250
}) {
    const apiLimit = 1000;
    const usage = Math.min(Math.max((apiCalls / apiLimit) * 100, 0), 100);

    return (
        <section className="dashboard-metrics-grid" aria-label="Dashboard metrics">
            <article className="metric-card metric-card--hazard">
                <div className="metric-header">
                    <span className="metric-title">7-day hazardous objects</span>
                    <span className="metric-symbol metric-symbol--danger" aria-hidden="true">!</span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number text-rose">{hazardousCount}</span>
                    <span className="metric-trend">Next 7 days</span>
                </div>
                <p className="metric-description">Potentially hazardous asteroids</p>
            </article>

            <article className="metric-card">
                <div className="metric-header">
                    <span className="metric-title">NASA NeoWS API usage</span>
                    <span className="status-indicator">
                        <span className="status-dot" aria-hidden="true" />
                        Operational
                    </span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number">
                        {apiCalls.toLocaleString()} <small className="metric-sub">/ {apiLimit.toLocaleString()}</small>
                    </span>
                    <span className="metric-trend text-cyan">{usage.toFixed(0)}%</span>
                </div>
                <div
                    className="metric-progress"
                    role="progressbar"
                    aria-label="API request usage"
                    aria-valuemin="0"
                    aria-valuemax={apiLimit}
                    aria-valuenow={Math.min(Math.max(apiCalls, 0), apiLimit)}
                >
                    <span style={{ width: `${usage}%` }} />
                </div>
                <p className="metric-description">Requests used in the current quota window</p>
            </article>

            <article className="metric-card">
                <div className="metric-header">
                    <span className="metric-title">Active catalog</span>
                    <span className="metric-symbol" aria-hidden="true">✦</span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number">{totalTracked.toLocaleString()}</span>
                    <span className="metric-trend">Tracked objects</span>
                </div>
                <p className="metric-description">Objects currently in the tracked catalog</p>
            </article>
        </section>
    );
}
