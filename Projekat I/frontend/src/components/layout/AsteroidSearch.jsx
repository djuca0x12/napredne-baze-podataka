import React, { useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidSearch.css';

export default function AsteroidSearch({ asteroids = [] }) {
    const [query, setQuery] = useState('');
    const [searchResult, setSearchResult] = useState(null);
    const [searched, setSearched] = useState(false);
    const [loading, setLoading] = useState(false);

    const handleSearch = (e) => {
        e.preventDefault();
        if (!query.trim()) return;

        setLoading(true);
        setSearched(true);

        // Simulate database/API latency for space telemetry search
        // todo: obrisati nakon spajanja backend-a!
        setTimeout(() => {
            const found = asteroids.find(
                (ast) =>
                    ast.name.toLowerCase().includes(query.toLowerCase().trim()) ||
                    ast.id.toLowerCase().includes(query.toLowerCase().trim())
            );
            setSearchResult(found || null);
            setLoading(false);
        }, 350);
    };

    return (
        <div className="asteroid-search-container">
            <div className="search-header">
                <div className="telemetry-badge">
                    <span className="pulsing-dot"></span>
                    NeoWS Database Query
                </div>
                <h2 className="search-title">Search Asteroid by Name or ID</h2>
            </div>

            <form onSubmit={handleSearch} className="search-form">
                <div className="search-input-wrapper">
                    <span className="search-icon">🔭</span>
                    <input
                        type="text"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        placeholder="e.g., Apophis, 2024-AB1, 99942..."
                        className="search-input"
                    />
                </div>
                <button type="submit" className="search-submit-btn" disabled={loading}>
                    {loading ? 'Scanning...' : 'Initialize Search'}
                </button>
            </form>

            {searched && !loading && !searchResult && (
                <div className="search-no-results">
                    <span>⚠️ No Near-Earth Object found matching "{query}". Check designation or try another query.</span>
                </div>
            )}

            {searchResult && (
                <div className="search-results-wrapper">
                    <AsteroidCard
                        asteroid={searchResult}
                        onClose={() => {
                            setSearchResult(null);
                            setSearched(false);
                            setQuery('');
                        }}
                    />
                </div>
            )}
        </div>
    );
}