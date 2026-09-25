CREATE TABLE IF NOT EXISTS elements (
    id SERIAL PRIMARY KEY,
    attribute_value TEXT,
    html TEXT NOT NULL
);