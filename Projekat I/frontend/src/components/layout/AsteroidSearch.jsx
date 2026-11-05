import React, { useEffect, useRef, useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidSearch.css';

export default function AsteroidSearch({ asteroids = [] }) {
    const [query, setQuery] = useState('');
    const [searchResult, setSearchResult] = useState(null);
    const [searched, setSearched] = useState(false);
    const [loading, setLoading] = useState(false);
    const timerRef = useRef(null);

    useEffect(() => () => window.clearTimeout(timerRef.current), []);

    const handleSearch = (event) => {
        event.preventDefault();
        const normalizedQuery = query.trim().toLowerCase();
        if (!normalizedQuery || loading) return;

        window.clearTimeout(timerRef.current);
        setLoading(true);
        setSearched(true);
        setSearchResult(null);

        // todo: obrisati nepotreban delay uz pretragu kad se poveže backend
        timerRef.current = window.setTimeout(() => {
            const found = asteroids.find((asteroid) =>
                asteroid.name.toLowerCase().includes(normalizedQuery) ||
                asteroid.id.toLowerCase().includes(normalizedQuery)
            );
            setSearchResult(found || null);
            setLoading(false);
        }, 350);
    };

    const clearSearch = () => {
        window.clearTimeout(timerRef.current);
        setSearchResult(null);
        setSearched(false);
        setLoading(false);
        setQuery('');
    };

    return (
        <section className="asteroid-search-container" aria-labelledby="asteroid-search-title">
            <div className="search-header">
                <div className="search-eyebrow">
                    <span className="search-radar" aria-hidden="true">⌕</span>
                    <span>NEOWS DATABASE</span>
                    <span className="search-demo-tag">DEMO CATALOG</span>
                </div>
                <h2 className="search-title" id="asteroid-search-title">Find a near-Earth object</h2>
                <p className="search-description">
                    Search the catalog by asteroid name or official designation.
                </p>
            </div>

            <form onSubmit={handleSearch} className="search-form" role="search">
                <label className="search-input-wrapper">
                    <span className="search-icon" aria-hidden="true">⌕</span>
                    <span className="sr-only">Asteroid name or ID</span>
                    <input
                        type="search"
                        value={query}
                        onChange={(event) => setQuery(event.target.value)}
                        placeholder="e.g. Apophis, 2024-AB1, 99942..."
                        className="search-input"
                        autoComplete="off"
                    />
                    {query && (
                        <button
                            type="button"
                            className="search-clear-btn"
                            onClick={clearSearch}
                            aria-label="Clear search"
                        >
                            ×
                        </button>
                    )}
                </label>
                <button
                    type="submit"
                    className="search-submit-btn"
                    disabled={loading || !query.trim()}
                >
                    {loading ? (
                        <><span className="loading-spinner" aria-hidden="true" /> Searching</>
                    ) : 'Search catalog'}
                </button>
            </form>

            <p className="search-hint">TIP: Press Enter to search · Names and object IDs are supported</p>

            <div className="search-feedback" aria-live="polite" aria-atomic="true">
                {searched && loading && (
                    <div className="search-loading">Searching the asteroid catalog…</div>
                )}
                {searched && !loading && !searchResult && (
                    <div className="search-no-results">
                        <strong>No matching object found</strong>
                        <span>Check the designation or try another name or ID.</span>
                    </div>
                )}
                {searchResult && (
                    <div className="search-results-wrapper">
                        <AsteroidCard asteroid={searchResult} onClose={clearSearch} />
                    </div>
                )}
            </div>
        </section>
    );
}
