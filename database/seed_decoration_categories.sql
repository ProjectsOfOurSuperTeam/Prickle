-- Seed decoration categories
-- Run after schema.sql, before decorations.sql

INSERT INTO decoration_categories (id, name, description) VALUES
(gen_random_uuid(), 'Каміння', 'Галька, лава, сланці, мінерали для декорування поверхні ґрунту.'),
(gen_random_uuid(), 'Пісок', 'Дрібнозернистий пісок різних кольорів для імітації пейзажів.'),
(gen_random_uuid(), 'Дерево', 'Коряги, кора та інші елементи з деревини.'),
(gen_random_uuid(), 'Фігурки', 'Мініатюрна архітектура, персонажі та декоративні об''єкти.'),
(gen_random_uuid(), 'Природа', 'Мушлі, природні об''єкти для акцентів.'),
(gen_random_uuid(), 'Мінерали', 'Кристали, друзи та інші мінеральні елементи.');
