
import React from 'react';
import '../../styles/AsteroidDashboardMetrics.css';

export default function AsteroidDashboardMetrics({
    hazardousCount = 0,
    apiCalls = 342,
    totalTracked = 0,
    totalViews = 0,
    totalSearches = 0,
    statisticsLoading = false
}) {
    const apiLimit = 1000;

    const usage = Math.min(
        Math.max((apiCalls / apiLimit) * 100, 0),
        100
    );

    const views = Math.max(0, totalViews);
    const searches = Math.max(0, totalSearches);

    // Relative activity comparison.
    const activityTotal = views + searches;

    const viewsPercentage = activityTotal > 0
        ? (views / activityTotal) * 100
        : 0;

    const searchesPercentage = activityTotal > 0
        ? (searches / activityTotal) * 100
        : 0;

    const formatNumber = value =>
        Number(value).toLocaleString();

    const displayStat = value =>
        statisticsLoading ? '...' : formatNumber(value);

    return (
        <section
            className="dashboard-section"
            aria-label="Dashboard metrics"
        >
            <div className="dashboard-metrics-grid">

                {/* Hazardous asteroids */}
                <article className="metric-card metric-card--hazard">
                    <div className="metric-header">
                        <span className="metric-title">
                            7-day hazardous objects
                        </span>

                        <span
                            className="metric-symbol metric-symbol--danger"
                            aria-hidden="true"
                        >
                            !
                        </span>
                    </div>

                    <div className="metric-value-row">
                        <span className="metric-number text-rose">
                            {formatNumber(hazardousCount)}
                        </span>

                        <span className="metric-trend">
                            Next 7 days
                        </span>
                    </div>

                    <p className="metric-description">
                        Potentially hazardous asteroids
                    </p>
                </article>

                {/* NASA API usage */}
                {/* <article className="metric-card">
                    <div className="metric-header">
                        <span className="metric-title">
                            NASA NeoWS API usage
                        </span>

                        <span className="status-indicator">
                            <span className="status-dot" />
                            Operational
                        </span>
                    </div>

                    <div className="metric-value-row">
                        <span className="metric-number">
                            {formatNumber(apiCalls)}
                            <small className="metric-sub">
                                {' '}/ {formatNumber(apiLimit)}
                            </small>
                        </span>

                        <span className="metric-trend text-cyan">
                            {usage.toFixed(0)}%
                        </span>
                    </div>

                    <div
                        className="metric-progress"
                        role="progressbar"
                        aria-label="API request usage"
                        aria-valuemin="0"
                        aria-valuemax={apiLimit}
                        aria-valuenow={Math.min(
                            Math.max(apiCalls, 0),
                            apiLimit
                        )}
                    >
                        <span style={{ width: `${usage}%` }} />
                    </div>

                    <p className="metric-description">
                        Requests used in the current quota window
                    </p>
                </article> */}

                {/* Tracked asteroids */}
                <article className="metric-card">
                    <div className="metric-header">
                        <span className="metric-title">
                            Active catalog
                        </span>

                        <span className="metric-symbol">
                            ✦
                        </span>
                    </div>

                    <div className="metric-value-row">
                        <span className="metric-number">
                            {formatNumber(totalTracked)}
                        </span>

                        <span className="metric-trend">
                            Tracked objects
                        </span>
                    </div>

                    <p className="metric-description">
                        Objects currently in the tracked catalog
                    </p>
                </article>

                {/* Total asteroid views */}
                <article className="metric-card metric-card--views">
                    <div className="metric-header">
                        <span className="metric-title">
                            Total asteroid views
                        </span>

                        <span
                            className="metric-symbol metric-symbol--views"
                            aria-hidden="true"
                        >
                            ◉
                        </span>
                    </div>

                    <div className="metric-value-row">
                        <span className="metric-number text-cyan">
                            {displayStat(views)}
                        </span>

                        <span className="metric-trend">
                            All time
                        </span>
                    </div>

                    <p className="metric-description">
                        Total detail-page views across all asteroids
                    </p>
                </article>

                {/* Total asteroid searches */}
                <article className="metric-card metric-card--searches">
                    <div className="metric-header">
                        <span className="metric-title">
                            Total searches
                        </span>

                        <span
                            className="metric-symbol metric-symbol--search"
                            aria-hidden="true"
                        >
                            ⌕
                        </span>
                    </div>

                    <div className="metric-value-row">
                        <span className="metric-number text-cyan">
                            {displayStat(searches)}
                        </span>

                        <span className="metric-trend">
                            All time
                        </span>
                    </div>

                    <p className="metric-description">
                        Successfully recorded asteroid searches
                    </p>
                </article>

            </div>

            {/* Activity comparison */}
            <article className="metric-card activity-card">
                <div className="activity-header">
                    <div>
                        <span className="metric-title">
                            APPLICATION ACTIVITY
                        </span>

                        <h3 className="activity-title">
                            Views vs. searches
                        </h3>
                    </div>

                    <span className="activity-total">
                        {statisticsLoading
                            ? 'Loading...'
                            : `${formatNumber(activityTotal)} events`}
                    </span>
                </div>

                <div
                    className="activity-bar"
                    role="img"
                    aria-label={`Views: ${viewsPercentage.toFixed(1)} percent. Searches: ${searchesPercentage.toFixed(1)} percent.`}
                >
                    <span
                        className="activity-bar-views"
                        style={{
                            width: `${viewsPercentage}%`
                        }}
                    />

                    <span
                        className="activity-bar-searches"
                        style={{
                            width: `${searchesPercentage}%`
                        }}
                    />
                </div>

                <div className="activity-legend">
                    <div className="activity-legend-item">
                        <span className="legend-dot legend-dot--views" />

                        <span>Detail views</span>

                        <strong>
                            {displayStat(views)}
                        </strong>
                    </div>

                    <div className="activity-legend-item">
                        <span className="legend-dot legend-dot--searches" />

                        <span>Searches</span>

                        <strong>
                            {displayStat(searches)}
                        </strong>
                    </div>
                </div>
            </article>
        </section>
    );
}
