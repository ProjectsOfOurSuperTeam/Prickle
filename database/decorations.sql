-- Seed decorations from Prickle catalog
-- Run after schema.sql and seed_decoration_categories.sql
-- Image paths: entities/decorations/<name>.png

INSERT INTO decorations (
    name,
    description,
    category_id,
    image,
    image_isometric
) VALUES

-- КАМІННЯ ТА ГАЛЬКА
(
    'Біла морська галька',
    'Гладеньке біле каміння середнього розміру. Створює яскравий контраст із зеленим мохом.',
    (SELECT id FROM decoration_categories WHERE name = 'Каміння' LIMIT 1),
    'entities/decorations/white_sea_gravel.png',
    'entities/decorations/white_sea_gravel_isometric.png'
),
(
    'Чорна вулканічна лава',
    'Пористе чорне каміння з грубою текстурою. Ідеально підходить для пустельних композицій.',
    (SELECT id FROM decoration_categories WHERE name = 'Каміння' LIMIT 1),
    'entities/decorations/volcanic_lava_rock.png',
    'entities/decorations/volcanic_lava_rock_isometric.png'
),
(
    'Сланцева крихта',
    'Пласкі шматочки сірого сланцю. Можна використовувати для створення імітації скель або східців.',
    (SELECT id FROM decoration_categories WHERE name = 'Каміння' LIMIT 1),
    'entities/decorations/slate_chips.png',
    'entities/decorations/slate_chips_isometric.png'
),
(
    'Червона яшма',
    'Природний мінерал насиченого теракотового кольору. Додає теплих відтінків флораріуму.',
    (SELECT id FROM decoration_categories WHERE name = 'Каміння' LIMIT 1),
    'entities/decorations/red_jasper.png',
    'entities/decorations/red_jasper_isometric.png'
),

-- ПІСОК ТА ГРУНТ
(
    'Блакитний кварцовий пісок',
    'Дрібнозернистий пісок насиченого кольору. Використовується для імітації води або річок.',
    (SELECT id FROM decoration_categories WHERE name = 'Пісок' LIMIT 1),
    'entities/decorations/blue_quartz_sand.png',
    'entities/decorations/blue_quartz_sand_isometric.png'
),
(
    'Золотистий пустельний пісок',
    'Натуральний чистий пісок для створення реалістичних пустельних пейзажів.',
    (SELECT id FROM decoration_categories WHERE name = 'Пісок' LIMIT 1),
    'entities/decorations/desert_gold_sand.png',
    'entities/decorations/desert_gold_sand_isometric.png'
),

-- ДЕРЕВО ТА КОРЯГИ
(
    'Дубова коряга "Дрифтвуд"',
    'Вивітрена водою деревина вигадливої форми. Нагадує старе дерево в мініатюрі.',
    (SELECT id FROM decoration_categories WHERE name = 'Дерево' LIMIT 1),
    'entities/decorations/oak_driftwood.png',
    'entities/decorations/oak_driftwood_isometric.png'
),
(
    'Кора соснова',
    'Натуральні шматочки кори для декорування поверхні ґрунту в лісових композиціях.',
    (SELECT id FROM decoration_categories WHERE name = 'Дерево' LIMIT 1),
    'entities/decorations/pine_bark.png',
    'entities/decorations/pine_bark_isometric.png'
),

-- МІНІ-АРХІТЕКТУРА
(
    'Класичний маяк',
    'Деталізована фігурка біло-червоного маяка. Центр композиції для морської тематики.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/classic_lighthouse.png',
    'entities/decorations/classic_lighthouse_isometric.png'
),
(
    'Японська брама Торії',
    'Червона ритуальна брама. Додає східного колориту та спокою вашому садочку.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/torii_gate.png',
    'entities/decorations/torii_gate_isometric.png'
),
(
    'Кам''яний місток',
    'Маленький вигнутий місток, що ідеально лягає над "річкою" з піску.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/stone_bridge.png',
    'entities/decorations/stone_bridge_isometric.png'
),
(
    'Будиночок хобіта',
    'Крихітні круглі двері в "пагорбі", прикрашені імітацією ліан.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/hobbit_house.png',
    'entities/decorations/hobbit_house_isometric.png'
),
(
    'Мініатюрна лава',
    'Дерев''яна садова лавка для створення затишного паркового куточка.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/mini_bench.png',
    'entities/decorations/mini_bench_isometric.png'
),

-- ПРИРОДА ТА ПЕРСОНАЖІ
(
    'Набір лісових грибів',
    'Три червоних мухомори різного розміру на спільній основі.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/forest_mushrooms.png',
    'entities/decorations/forest_mushrooms_isometric.png'
),
(
    'Спляче лисеня',
    'Маленька помаранчева фігурка лисиці, що згорнулася клубочком.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/sleeping_fox.png',
    'entities/decorations/sleeping_fox_isometric.png'
),
(
    'Керамічна панда',
    'Маленька панда, що жує бамбук. Добре виглядає поруч із сукулентами.',
    (SELECT id FROM decoration_categories WHERE name = 'Фігурки' LIMIT 1),
    'entities/decorations/panda_figure.png',
    'entities/decorations/panda_figure_isometric.png'
),
(
    'Морська мушля "Рапана"',
    'Натуральна мушля невеликого розміру для акцентів у відкритих формах.',
    (SELECT id FROM decoration_categories WHERE name = 'Природа' LIMIT 1),
    'entities/decorations/sea_shell.png',
    'entities/decorations/sea_shell_isometric.png'
),

-- КРИСТАЛИ
(
    'Друза аметисту',
    'Натуральний фіолетовий кристал. Додає магічного вигляду та блиску під світлом.',
    (SELECT id FROM decoration_categories WHERE name = 'Мінерали' LIMIT 1),
    'entities/decorations/amethyst_cluster.png',
    'entities/decorations/amethyst_cluster_isometric.png'
),
(
    'Прозорий гірський кришталь',
    'Вертикальний гострий кристал, що імітує крижану скелю.',
    (SELECT id FROM decoration_categories WHERE name = 'Мінерали' LIMIT 1),
    'entities/decorations/quartz_crystal.png',
    'entities/decorations/quartz_crystal_isometric.png'
),
(
    'Бурштинова крихта',
    'Дрібні прозорі камінці медового кольору для розсипання по ґрунту.',
    (SELECT id FROM decoration_categories WHERE name = 'Мінерали' LIMIT 1),
    'entities/decorations/amber_chips.png',
    'entities/decorations/amber_chips_isometric.png'
);
