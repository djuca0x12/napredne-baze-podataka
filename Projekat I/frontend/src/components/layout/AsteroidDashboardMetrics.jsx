import React from 'react';
import '../../styles/AsteroidDashboardMetrics.css';

export default function AsteroidDashboardMetrics({
    hazardousCount = 14,
    apiCalls = 342,
    totalTracked = 1250
}) {
    return (
        <div className="dashboard-metrics-grid">

            {/* 7-Day Hazardous Objects Card */}
            <div className="metric-card">
                <div className="metric-header">
                    <span className="metric-title">7-Day Hazardous (PHA)</span>
                    <span className="metric-icon">⚠️</span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number text-rose">{hazardousCount}</span>
                    <span className="metric-trend">Next 7 Days</span>
                </div>
            </div>

            {/* API Usage Counter Card */}
            <div className="metric-card">
                <div className="metric-header">
                    <span className="metric-title">NASA NeoWS API Usage</span>
                    <span className="metric-icon"><span className="status-dot"></span></span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number">{apiCalls} <small className="metric-sub">/ 1k</small></span>
                    <span className="metric-trend text-cyan">Operational</span>
                </div>
            </div>

            {/* Active Catalog Card */}
            <div className="metric-card">
                <div className="metric-header">
                    <span className="metric-title">Active Catalog</span>
                    <span className="metric-icon">🛰️</span>
                </div>
                <div className="metric-value-row">
                    <span className="metric-number">{totalTracked}</span>
                    <span className="metric-trend">Tracked Objects</span>
                </div>
            </div>

        </div>
    );
}