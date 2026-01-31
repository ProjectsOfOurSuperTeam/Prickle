-- Prickle PostgreSQL Schema
-- Images stored as BYTEA (blob)

-- Enums
CREATE TYPE user_role AS ENUM ('User', 'Admin');
CREATE TYPE decoration_category AS ENUM ('Каміння', 'Пісок', 'Фігурки');
CREATE TYPE project_item_type AS ENUM ('Plant', 'Decoration', 'Soil');

-- Users
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    role user_role NOT NULL DEFAULT 'User',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Soil Formulas (soil types for plants)
CREATE TABLE soil_formulas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    recipe_json JSONB NOT NULL,
    description TEXT
);

-- Plants (Plant catalog)
CREATE TABLE plants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name_ua VARCHAR(255) NOT NULL,
    name_latin VARCHAR(255) NOT NULL,
    description TEXT,
    light_level INT NOT NULL CHECK (light_level BETWEEN 1 AND 5),
    water_need INT NOT NULL CHECK (water_need BETWEEN 1 AND 5),
    humidity_level INT NOT NULL CHECK (humidity_level BETWEEN 1 AND 5),
    max_size FLOAT NOT NULL,
    soil_formula_id UUID NOT NULL REFERENCES soil_formulas(id),
    image VARCHAR(500),
    image_isometric VARCHAR(500)
);

CREATE INDEX ix_plants_soil_formula_id ON plants(soil_formula_id);

-- Containers (Glass forms)
CREATE TABLE containers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    volume_liters FLOAT NOT NULL,
    is_closed BOOLEAN NOT NULL DEFAULT FALSE,
    image_base BYTEA,
    image_mask BYTEA
);

-- Decorations
CREATE TABLE decorations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category decoration_category NOT NULL,
    image BYTEA,
    image_isometric BYTEA
);

-- Projects (Saved designs)
CREATE TABLE projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    container_id UUID NOT NULL REFERENCES containers(id),
    preview BYTEA,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_published BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX ix_projects_user_id ON projects(user_id);
CREATE INDEX ix_projects_container_id ON projects(container_id);
CREATE INDEX ix_projects_created_at ON projects(created_at DESC);

-- Project Items (Project composition - where each plant/decor/soil sits on canvas)
CREATE TABLE project_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    item_type project_item_type NOT NULL,
    item_id UUID NOT NULL,
    pos_x FLOAT NOT NULL,
    pos_y FLOAT NOT NULL,
    z_index INT NOT NULL
);

CREATE INDEX ix_project_items_project_id ON project_items(project_id);

-- Likes (Composite PK)
CREATE TABLE likes (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, project_id)
);

CREATE INDEX ix_likes_project_id ON likes(project_id);
