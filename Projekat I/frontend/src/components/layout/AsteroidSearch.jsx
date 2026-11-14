
import React, { useEffect, useRef, useState } from 'react';
import AsteroidCard from './AsteroidCard';
import '../../styles/AsteroidSearch.css';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

function normalizeAsteroid(asteroid) {
    const approaches = asteroid.approaches ?? [];

    const speeds = approaches
        .map(a => Number(a.relativeVelocityKps))
        .filter(Number.isFinite);

    const distances = approaches
        .map(a => Number(a.missDistance))
        .filter(Number.isFinite);

    return {
        ...asteroid,

        speedKmS: speeds.length
            ? Math.max(...speeds)
            : 0,

        diameterKm: Number(asteroid.diameterMaxKm ?? 0),

        missDistanceKm: distances.length
            ? Math.min(...distances)
            : 0,

        hazardous: asteroid.isHazardous ?? false,
        orbitClass: asteroid.orbitClass ?? 'Unknown'
    };
}

export default function AsteroidSearch() {
    const [query, setQuery] = useState('');

    const [results, setResults] = useState([]);
    const [selectedAsteroid, setSelectedAsteroid] = useState(null);

    const [searched, setSearched] = useState(false);
    const [loading, setLoading] = useState(false);
    const [detailsLoading, setDetailsLoading] = useState(false);

    const [error, setError] = useState(null);

    const timerRef = useRef(null);
    const searchControllerRef = useRef(null);
    const detailsControllerRef = useRef(null);

    useEffect(() => {
        return () => {
            window.clearTimeout(timerRef.current);
            searchControllerRef.current?.abort();
            detailsControllerRef.current?.abort();
        };
    }, []);

    const handleSearch = async (event) => {
        event.preventDefault();

        const normalizedQuery = query.trim();

        if (!normalizedQuery || loading) return;

        searchControllerRef.current?.abort();
        detailsControllerRef.current?.abort();

        const controller = new AbortController();
        searchControllerRef.current = controller;

        setLoading(true);
        setSearched(true);
        setResults([]);
        setSelectedAsteroid(null);
        setError(null);

        try {
            const response = await fetch(
                `${API_BASE_URL}/Asteroid/name?name=${encodeURIComponent(normalizedQuery)}`,
                { signal: controller.signal }
            );

            if (!response.ok) {
                throw new Error(
                    `Search failed: ${response.status}`
                );
            }

            const data = await response.json();

            const formattedResults = data.map(normalizeAsteroid);

            setResults(formattedResults);

        } catch (err) {
            if (err.name !== 'AbortError') {
                console.error('Asteroid search error:', err);
                setError(err.message);
            }
        } finally {
            if (!controller.signal.aborted) {
                setLoading(false);
            }
        }
    };

    // otvaranjem jednog asteroida, inkrementira broj pregleda preko AsteroidController.GetAsteroidById
    const handleSelectAsteroid = async (id) => {
        detailsControllerRef.current?.abort();

        const controller = new AbortController();
        detailsControllerRef.current = controller;

        setDetailsLoading(true);
        setSelectedAsteroid(null);
        setError(null);

        try {
            const response = await fetch(
                `${API_BASE_URL}/Asteroid/id?id=${encodeURIComponent(id)}`,
                { signal: controller.signal }
            );

            if (!response.ok) {
                throw new Error(
                    `Unable to load asteroid details: ${response.status}`
                );
            }

            const data = await response.json();

            setSelectedAsteroid(normalizeAsteroid(data));

        } catch (err) {
            if (err.name !== 'AbortError') {
                console.error('Asteroid details error:', err);
                setError(err.message);
            }
        } finally {
            if (!controller.signal.aborted) {
                setDetailsLoading(false);
            }
        }
    };

    const clearSearch = () => {
        searchControllerRef.current?.abort();
        detailsControllerRef.current?.abort();

        setResults([]);
        setSelectedAsteroid(null);
        setSearched(false);
        setLoading(false);
        setDetailsLoading(false);
        setError(null);
        setQuery('');
    };

    return (
        <section
            className="asteroid-search-container"
            aria-labelledby="asteroid-search-title"
        >
            <div className="search-header">
                <div className="search-eyebrow">
                    <span className="search-radar" aria-hidden="true">
                        ⌕
                    </span>

                    <span>NEOWS DATABASE</span>

                    <span className="search-demo-tag">
                        LIVE DATABASE
                    </span>
                </div>

                <h2
                    className="search-title"
                    id="asteroid-search-title"
                >
                    Find a near-Earth object
                </h2>

                <p className="search-description">
                    Search the asteroid database by name or official
                    designation. All matching objects will be displayed.
                </p>
            </div>

            <form
                onSubmit={handleSearch}
                className="search-form"
                role="search"
            >
                <label className="search-input-wrapper">
                    <span className="search-icon" aria-hidden="true">
                        ⌕
                    </span>

                    <span className="sr-only">
                        Asteroid name or ID
                    </span>

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
                        <>
                            <span
                                className="loading-spinner"
                                aria-hidden="true"
                            />
                            Searching
                        </>
                    ) : 'Search catalog'}
                </button>
            </form>

            <p className="search-hint">
                TIP: Press Enter to search · Names and object IDs are supported
            </p>

            <div
                className="search-feedback"
                aria-live="polite"
                aria-atomic="true"
            >
                {loading && (
                    <div className="search-loading">
                        Searching the asteroid database…
                    </div>
                )}

                {error && (
                    <div className="search-no-results">
                        <strong>Request failed</strong>
                        <span>{error}</span>
                    </div>
                )}

                {searched && !loading && !error && (
                    <div className="search-results-heading">
                        <span>SEARCH RESULTS</span>

                        <strong>
                            {results.length} matching objects
                        </strong>
                    </div>
                )}

                {!loading && searched && !error && results.length === 0 && (
                    <div className="search-no-results">
                        <strong>No matching objects found</strong>
                        <span>
                            Try another asteroid name or designation.
                        </span>
                    </div>
                )}

                {results.length > 0 && (
                    <div className="search-results-list">
                        {results.map((asteroid) => (
                            <button
                                type="button"
                                key={asteroid.id}
                                className={`search-result-item ${selectedAsteroid?.id === asteroid.id
                                        ? 'search-result-item--active'
                                        : ''
                                    }`}
                                onClick={() => handleSelectAsteroid(asteroid.id)}
                                disabled={detailsLoading}
                            >
                                <span className="search-result-main">
                                    <strong>{asteroid.name}</strong>

                                    <span>
                                        ID: {asteroid.id}
                                    </span>
                                </span>

                                <span className="search-result-meta">
                                    {asteroid.hazardous && (
                                        <span className="search-result-hazard">
                                            PHA
                                        </span>
                                    )}

                                    <span className="search-result-diameter">
                                        {Number(asteroid.diameterKm).toFixed(2)} km
                                    </span>

                                    <span aria-hidden="true">→</span>
                                </span>
                            </button>
                        ))}
                    </div>
                )}

                {detailsLoading && (
                    <div className="search-loading">
                        Loading asteroid details…
                    </div>
                )}

                {selectedAsteroid && (
                    <div className="search-results-wrapper">
                        <AsteroidCard
                            key={selectedAsteroid.id}
                            asteroid={selectedAsteroid}
                            onClose={() => setSelectedAsteroid(null)}
                        />
                    </div>
                )}
            </div>
        </section>
    );
}
