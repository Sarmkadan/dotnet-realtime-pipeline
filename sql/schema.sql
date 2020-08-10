-- =============================================================================
-- PostgreSQL Schema for dotnet-realtime-pipeline
-- Optional persistent storage for data points and metrics
-- =============================================================================

-- Create extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- DataPoints table - stores individual data points
CREATE TABLE IF NOT EXISTS data_points (
    id BIGSERIAL PRIMARY KEY,
    guid UUID UNIQUE DEFAULT uuid_generate_v4(),
    timestamp BIGINT NOT NULL,
    value NUMERIC NOT NULL,
    source VARCHAR(255) NOT NULL,
    quality DECIMAL(5,4) DEFAULT 1.0 CHECK (quality >= 0 AND quality <= 1),
    is_valid BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for common queries
CREATE INDEX idx_data_points_timestamp ON data_points(timestamp DESC);
CREATE INDEX idx_data_points_source ON data_points(source);
CREATE INDEX idx_data_points_quality ON data_points(quality);
CREATE INDEX idx_data_points_timestamp_source ON data_points(timestamp DESC, source);
CREATE INDEX idx_data_points_timestamp_quality ON data_points(timestamp DESC, quality);

-- Windows table - stores aggregated window data
CREATE TABLE IF NOT EXISTS windows (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    window_type VARCHAR(50) NOT NULL,
    start_time_ms BIGINT NOT NULL,
    end_time_ms BIGINT NOT NULL,
    count INT DEFAULT 0,
    sum_value NUMERIC,
    avg_value NUMERIC,
    min_value NUMERIC,
    max_value NUMERIC,
    stddev_value NUMERIC,
    percentile_50 NUMERIC,
    percentile_95 NUMERIC,
    percentile_99 NUMERIC,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_windows_start_time ON windows(start_time_ms DESC);
CREATE INDEX idx_windows_type ON windows(window_type);
CREATE INDEX idx_windows_time_range ON windows(start_time_ms, end_time_ms);

-- Metrics table - stores aggregated metrics
CREATE TABLE IF NOT EXISTS metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    measurement_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    throughput_items_per_sec NUMERIC,
    avg_latency_ms NUMERIC,
    min_latency_ms NUMERIC,
    max_latency_ms NUMERIC,
    error_rate DECIMAL(5,4),
    buffer_utilization DECIMAL(5,4),
    memory_usage_mb NUMERIC,
    health_status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_metrics_measurement_time ON metrics(measurement_time DESC);
CREATE INDEX idx_metrics_health_status ON metrics(health_status);
CREATE INDEX idx_metrics_time_range ON metrics(measurement_time DESC) WHERE measurement_time > CURRENT_TIMESTAMP - INTERVAL '24 hours';

-- Processing results table - tracks processing outcomes
CREATE TABLE IF NOT EXISTS processing_results (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    data_point_id BIGINT REFERENCES data_points(id),
    status VARCHAR(50) NOT NULL,
    quality_score DECIMAL(5,4),
    is_outlier BOOLEAN DEFAULT false,
    error_message TEXT,
    processing_time_ms NUMERIC,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_processing_results_data_point_id ON processing_results(data_point_id);
CREATE INDEX idx_processing_results_status ON processing_results(status);
CREATE INDEX idx_processing_results_created_at ON processing_results(created_at DESC);

-- Alerts table - stores alert history
CREATE TABLE IF NOT EXISTS alerts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    alert_type VARCHAR(100) NOT NULL,
    severity VARCHAR(50) NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    metadata JSONB,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alerts_alert_type ON alerts(alert_type);
CREATE INDEX idx_alerts_severity ON alerts(severity);
CREATE INDEX idx_alerts_resolved_at ON alerts(resolved_at) WHERE resolved_at IS NULL;
CREATE INDEX idx_alerts_created_at ON alerts(created_at DESC);

-- Pipeline state table - stores pipeline configuration snapshots
CREATE TABLE IF NOT EXISTS pipeline_state (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    pipeline_name VARCHAR(255) NOT NULL,
    version VARCHAR(50),
    state_data JSONB NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(pipeline_name, version)
);

CREATE INDEX idx_pipeline_state_pipeline_name ON pipeline_state(pipeline_name);
CREATE INDEX idx_pipeline_state_is_active ON pipeline_state(is_active);

-- Materialized view for recent metrics summary
CREATE MATERIALIZED VIEW IF NOT EXISTS v_recent_metrics_summary AS
SELECT
    AVG(throughput_items_per_sec) as avg_throughput,
    AVG(avg_latency_ms) as avg_latency,
    AVG(error_rate) as avg_error_rate,
    MIN(measurement_time) as window_start,
    MAX(measurement_time) as window_end,
    COUNT(*) as data_points
FROM metrics
WHERE measurement_time > CURRENT_TIMESTAMP - INTERVAL '1 hour'
GROUP BY DATE_TRUNC('minute', measurement_time)
ORDER BY window_start DESC;

CREATE INDEX idx_v_recent_metrics_summary_window_start
    ON v_recent_metrics_summary(window_start DESC);

-- Materialized view for data quality summary
CREATE MATERIALIZED VIEW IF NOT EXISTS v_data_quality_summary AS
SELECT
    source,
    DATE_TRUNC('hour', TO_TIMESTAMP(timestamp / 1000000.0)) as hour,
    COUNT(*) as total_points,
    COUNT(CASE WHEN is_valid THEN 1 END) as valid_points,
    AVG(quality) as avg_quality,
    COUNT(CASE WHEN is_valid = false THEN 1 END) as invalid_points
FROM data_points
WHERE timestamp > EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - INTERVAL '24 hours')) * 1000000
GROUP BY source, DATE_TRUNC('hour', TO_TIMESTAMP(timestamp / 1000000.0));

CREATE INDEX idx_v_data_quality_summary_source
    ON v_data_quality_summary(source, hour DESC);

-- Function to clean up old data
CREATE OR REPLACE FUNCTION cleanup_old_data(retention_days INT DEFAULT 30)
RETURNS TABLE(deleted_data_points BIGINT, deleted_metrics BIGINT) AS $$
DECLARE
    v_deleted_data_points BIGINT;
    v_deleted_metrics BIGINT;
BEGIN
    DELETE FROM processing_results
    WHERE created_at < CURRENT_TIMESTAMP - (retention_days || ' days')::INTERVAL;

    GET DIAGNOSTICS v_deleted_data_points = ROW_COUNT;

    DELETE FROM metrics
    WHERE created_at < CURRENT_TIMESTAMP - (retention_days || ' days')::INTERVAL;

    GET DIAGNOSTICS v_deleted_metrics = ROW_COUNT;

    RETURN QUERY SELECT v_deleted_data_points, v_deleted_metrics;
END;
$$ LANGUAGE plpgsql;

-- Function to update metrics
CREATE OR REPLACE FUNCTION update_metrics_summary()
RETURNS VOID AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY v_recent_metrics_summary;
    REFRESH MATERIALIZED VIEW CONCURRENTLY v_data_quality_summary;
END;
$$ LANGUAGE plpgsql;

-- Grant permissions to application user
GRANT SELECT, INSERT, UPDATE ON data_points TO pipeline;
GRANT SELECT, INSERT, UPDATE ON windows TO pipeline;
GRANT SELECT, INSERT ON metrics TO pipeline;
GRANT SELECT, INSERT ON processing_results TO pipeline;
GRANT SELECT, INSERT, UPDATE ON alerts TO pipeline;
GRANT SELECT, INSERT, UPDATE ON pipeline_state TO pipeline;
GRANT SELECT ON v_recent_metrics_summary TO pipeline;
GRANT SELECT ON v_data_quality_summary TO pipeline;
GRANT EXECUTE ON FUNCTION cleanup_old_data TO pipeline;
GRANT EXECUTE ON FUNCTION update_metrics_summary TO pipeline;
