    INSERT INTO plants (
    name_ua,
    name_latin,
    description,
    light_level,
    water_need,
    humidity_level,
    max_size,
    soil_formula_id,
    image,
    image_isometric
) VALUES
-- Ехеверія (Echeveria elegans)
(
    'Ехеверія елеганс',
    'Echeveria elegans',
    'Популярний сукулент з трояндовою розеткою сіро-блакитних листків. Любить яскраве світло та рідкісний полив.',
    5,
    1,
    1,
    0.15,
    (SELECT id FROM soil_formulas WHERE name = 'Пустельний мікс (Сукулент)' LIMIT 1),
    decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', 'base64'),
    NULL
),
