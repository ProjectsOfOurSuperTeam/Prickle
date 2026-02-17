using Microsoft.EntityFrameworkCore;
using Prickle.Domain.Containers;
using Prickle.Domain.Decorations;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;
using Prickle.Domain.Soil;

namespace Prickle.Infrastructure.Database;

public static class DbSeeder
{
    public static void SeedAll(DbContext context)
    {
        SeedSoilTypes(context);
        SeedSoilFormulas(context);
        SeedContainers(context);
        SeedDecorations(context);
        SeedPlants(context);
    }
    public static void SeedPlants(DbContext context)
    {
        if (context.Set<Plant>().Any())
        {
            return;
        }

        var formulas = context.Set<SoilFormulas>()
            .ToDictionary(f => f.Name, f => f.Id);

        Guid Formula(string name) => formulas[name];

        var desert = Formula("Пустельний мікс (Сукулент)");
        var arid = Formula("Аридний мінеральний");
        var tropical = Formula("Тропічний вологий (Класика)");
        var forest = Formula("Лісовий мох (Мосаріум)");
        var fern = Formula("Папоротевий легкий");
        var carniv = Formula("Хижий (Для венериної мухоловки)");
        var hardy = Formula("Універсальний Hardy");
        var begonia = Formula("Бегонієвий (Пухкий)");
        var aroid = Formula("Ароїдний (Aroid Mix)");

        var plants = new[]
        {
            // ===== СУКУЛЕНТИ =====
            Plant.Create(
                name: "Ехеверія Елеганс",
                nameLatin: "Echeveria elegans",
                description: "Сукулент з родини товстолистих (Crassulaceae), родом з напівпустель центральної Мексики. Формує щільну розетку діаметром до 10 см з лопатоподібних листків сіро-блакитного або сріблясто-зеленого кольору з восковим нальотом (фариною), який захищає від надмірного випаровування. Листя товсте, м'ясисте, загострене на кінці, злегка вгнуте всередину. Квітконоси висотою до 25 см несуть рожеві або коралові квіти-дзвіночки з жовтою серединкою. Нагадує кам'яну троянду — один з найпопулярніших сукулентів для флораріумів. Догляд: яскраве розсіяне світло, рідкий полив (раз на 2–3 тижні влітку), дренаж обов'язковий. Назва на честь мексиканського художника Атанасіо Ечеверріа-і-Годой, який ілюстрував ботанічні атласи XIX століття.",
                imageUrl: "entities/plants/echeveria_elegans.png",
                imageIsometricUrl: "entities/plants/echeveria_elegans_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.10
                soilFormulaId: desert),

            Plant.Create(
                name: "Хавортія Смугаста",
                nameLatin: "Haworthia fasciata",
                description: "Сукулент з родини асфоделових (Asphodelaceae), ендемік Капської провінції Південної Африки. Листя товсте, трикутне, темно-зелене з білими поперечними смугами-горбками знизу, зібране в щільну розетку діаметром до 12 см. Поверхня шорстка, на дотик приємна. Нагадує мініатюрне алое. Квіти білуваті, трубчасті, на тонкому квітконосі до 30 см. Ріст повільний — ідеально для довготривалого флораріуму. Догляд: помірне світло або легка півтінь, полив рідко, переносить сухе повітря. Назва на честь англійського ботаніка Едріана Гарді Готорна (Haworth).",
                imageUrl: "entities/plants/haworthia_fasciata.png",
                imageIsometricUrl: "entities/plants/haworthia_fasciata_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.Low,
                itemMaxSize: ProjectItemSize.Medium, // 0.12
                soilFormulaId: desert),

            Plant.Create(
                name: "Красула Хоббіт",
                nameLatin: "Crassula ovata Hobbit",
                description: "Сорт грошового дерева (Crassula ovata) з трубчастим листям, що закручується назовні — нагадує вушка Шрека або корали. Листя м'ясисте, яскраво-зелене, з червонуватим відтінком на сонці. Стебло з часом дерев'яніє, формує компактне деревце. Максимальна висота в контейнері — 15–20 см. Догляд: яскраве світло, рідкий полив, добре дренований ґрунт. Сорт виведений у 1970-х у США. Назва «Хоббіт» — відсилання до персонажів Толкіна. Ідеальний акцентний елемент у сукулентному флораріумі.",
                imageUrl: "entities/plants/crassula_ovata_hobbit.png",
                imageIsometricUrl: "entities/plants/crassula_ovata_hobbit_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: desert),

            Plant.Create(
                name: "Седум Рубротінктум",
                nameLatin: "Sedum rubrotinctum",
                description: "Мексиканський сукулент з дрібним листям у формі «желейних бобів», яке на сонці набуває яскраво-червоного або коралового кольору. Листя зібране в щільні розетки на повзучих стеблах. Швидко заповнює простір, створюючи килимок. Висота до 15 см. Догляд: максимум світла для інтенсивного забарвлення, полив помірний. Токсичний при вживанні. Латинська назва rubrotinctum означає «забарвлений у червоне».",
                imageUrl: "entities/plants/sedum_rubrotinctum.png",
                imageIsometricUrl: "entities/plants/sedum_rubrotinctum_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.12
                soilFormulaId: desert),

            Plant.Create(
                name: "Алое Остисте",
                nameLatin: "Aloe aristata",
                description: "Мініатюрне алое з Південної Африки. Листя темно-зелене з білими цятками та тонкими м'якими «вусиками» на кінчиках — характерна ознака виду. Формує щільну розетку до 15 см. Квіти коралово-помаранчеві, трубчасті, на високому квітконосі. Дуже витривале — витримує недогляд, сухе повітря, рідкісний полив. Догляд: яскраве світло, полив раз на 2 тижні. Вид описаний у 1824 році. Ідеально для початківців.",
                imageUrl: "entities/plants/aloe_aristata.png",
                imageIsometricUrl: "entities/plants/aloe_aristata_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: desert),

            Plant.Create(
                name: "Адроміскус Купера",
                nameLatin: "Adromischus cooperi",
                description: "Сукулент з родини товстолистих, ендемік Східнокапської провінції ПАР. Листя хвилясте, плямисте, товсте, лопатоподібне. Зібране в розетку до 8 см. Має дуже незвичну текстуру для переднього плану композиції. Квіти трубчасті, рожеві з білою серединкою. Догляд: яскраве світло, рідкий полив, обережно з переливом — схильний до гниття. Описаний у 1862 році ботаніком Джоном Гілбертом Бейкером.",
                imageUrl: "entities/plants/adromischus_cooperi.png",
                imageIsometricUrl: "entities/plants/adromischus_cooperi_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.10 — compact rosette ≤8cm
                soilFormulaId: desert),

            Plant.Create(
                name: "Сенеціо Роулі (Нитки перлів)",
                nameLatin: "Senecio rowleyanus",
                description: "Ампельний сукулент з Намібії та ПАР. Листя сферичне, діаметром 6–8 мм, зібране на тонких ниткоподібних стеблах — нагадує намисто з перлів. Колір світло-зелений з прозорою смугою («віконцем») для фотосинтезу. Швидко росте, гарно звисає з країв форми. Квіти білі з корицевим ароматом. Догляд: яскраве розсіяне світло, полив помірний. Токсичний! Один з найпопулярніших ампельних сукулентів у світі.",
                imageUrl: "entities/plants/senecio_rowleyanus.png",
                imageIsometricUrl: "entities/plants/senecio_rowleyanus_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.Low,
                itemMaxSize: ProjectItemSize.Large, // 0.20
                soilFormulaId: desert),

            Plant.Create(
                name: "Пахіфітум яйценосний (\"Місячний камінь\")",
                nameLatin: "Pachyphytum oviferum",
                description: "Мексиканський сукулент з дуже товстим, округлим листям, вкритим густим восковим нальотом (пруїном) — надає вигляду світло-сірого, рожевого або лавандового каміння. Листя до 4 см завдовжки. Розетка компактна. Квіти дрібні, червонувато-білі. Догляд: максимум світла, мінімум води, обережно з дотиком — нальот стирається. Вважається одним з найкрасивіших сукулентів для колекцій.",
                imageUrl: "entities/plants/pachyphytum_oviferum.png",
                imageIsometricUrl: "entities/plants/pachyphytum_oviferum_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.10
                soilFormulaId: desert),

            Plant.Create(
                name: "Граптопеталум парагвайський",
                nameLatin: "Graptopetalum paraguayense",
                description: "Сукулент з Мексики. Листя утворює розетки перламутрового сіро-блакитного кольору з восковим нальотом. На яскравому сонці набуває рожевого або фіолетового відтінку. Листя лопатоподібне, товсте. Квіти білі з червоними крапками. Догляд: яскраве світло, рідкий полив. Швидко розмножується листками. Іноді плутають з ехеверією.",
                imageUrl: "entities/plants/graptopetalum_paraguayense.png",
                imageIsometricUrl: "entities/plants/graptopetalum_paraguayense_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.12
                soilFormulaId: desert),

            Plant.Create(
                name: "Анакампсерос руфусценс",
                nameLatin: "Anacampseros rufescens",
                description: "Компактний сукулент з ПАР. Розетка з м'ясистим листям, яке знизу має насичений фіолетовий колір. Між листям часто ростуть тонкі білі волоски (аксили). Квіти рожеві, відкриваються вдень. Догляд: яскраве світло для фіолетового відтінку, полив рідко. Anacampseros з грецької — «рослина, що повертає любов». Рідкісний у колекціях, але дуже декоративний.",
                imageUrl: "entities/plants/anacampseros_rufescens.png",
                imageIsometricUrl: "entities/plants/anacampseros_rufescens_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.08
                soilFormulaId: desert),

            Plant.Create(
                name: "Фаукарія тигрова",
                nameLatin: "Faucaria tigrina",
                description: "Сукулент з ПАР з коротким стеблом та парами товстих трикутних листків. По краях листя — м'які зубці, що нагадують розкриту пащу тигра. Листя темно-зелене з білими крапками. Розетка до 10 см. Квіти жовті, великі, схожі на кульбабку. Догляд: яскраве світло, полив рідко влітку, майже сухо взимку. Faucaria — від лат. fauces («паща»).",
                imageUrl: "entities/plants/faucaria_tigrina.png",
                imageIsometricUrl: "entities/plants/faucaria_tigrina_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.08
                soilFormulaId: desert),

            Plant.Create(
                name: "Гастерія двоколірна",
                nameLatin: "Gasteria bicolor",
                description: "Сукулент з родини асфоделових (Asphodelaceae), ендемік Південної Африки. Має язикоподібне жорстке листя з білими бородавками, розташованими рядами. Листя темно-зелене зі світло-зеленими плямами, довге, загострене. Формує розетку до 20 см. Квіти трубчасті, рожеві або коралові. Добре переносить легку півтінь. Догляд: півтінь або розсіяне світло, полив рідко. Gasteria — «шлункоподібна» (за формою квіток).",
                imageUrl: "entities/plants/gasteria_bicolor.png",
                imageIsometricUrl: "entities/plants/gasteria_bicolor_isometric.png",
                category: PlantCategory.Succulents,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.Low,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.12
                soilFormulaId: desert),

            // ===== КАКТУСИ =====
            Plant.Create(
                name: "Літопс (\"Живе каміння\")",
                nameLatin: "Lithops spp.",
                description: "Сукулент з родини аїзових (Aizoaceae), родом з пустель Південної Африки. Два зрощені м'ясисті листки формують тіло, схоже на розрізану гальку — мімікрія під каміння для захисту від травоїдних. Щороку стара пара листків відмирає, нова проростає з щілини. Квіти білі або жовті, схожі на ромашки, з'являються восени. Догляд: максимум світла, мінімум води, особливо в період линяння. Lithops з грецької — «каміння-обличчя».",
                imageUrl: "entities/plants/lithops_spp.png",
                imageIsometricUrl: "entities/plants/lithops_spp_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.04
                soilFormulaId: arid),

            Plant.Create(
                name: "Плейоспілос Нелі",
                nameLatin: "Pleiospilos nelii",
                description: "Ще один вид «живого каміння» з ПАР. Схожий на розколотий гранітний валун. Зазвичай має два дуже товстих листки з крапчастим малюнком. Квіти великі, жовті або помаранчеві, з приємним ароматом. Догляд: екстремально яскраве світло, полив дуже рідко — лише коли старі листя зморщуються. Схильний до гниття при переливі. Pleiospilos — «багато плям». Описаний у 1926 році.",
                imageUrl: "entities/plants/pleiospilos_nelii.png",
                imageIsometricUrl: "entities/plants/pleiospilos_nelii_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.07
                soilFormulaId: arid),

            Plant.Create(
                name: "Мамілярія подовжена",
                nameLatin: "Mammillaria elongata",
                description: "Кактус з Мексики циліндричної форми. Стебло подовжене, до 20 см, вкрите золотистими або коричневими колючками. Часто цвіте дрібними кремовими або рожевими квітами, що формують вінок на верхівці. Плоди червоні, довгасті. Догляд: яскраве світло, полив рідко. Mammillaria — найбільший рід кактусів (понад 200 видів). Один з найпопулярніших кактусів для колекцій з XVIII століття.",
                imageUrl: "entities/plants/mammillaria_elongata.png",
                imageIsometricUrl: "entities/plants/mammillaria_elongata_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.10
                soilFormulaId: arid),

            Plant.Create(
                name: "Гімнокаліціум Міхановича",
                nameLatin: "Gymnocalycium mihanovichii",
                description: "Кулястий кактус з Парагваю та Аргентини. Має виражені ребра з горбками. При щепленні на зелену підщепу отримують безхлорофільні мутанти яскраво-червоного, жовтого або помаранчевого кольору — «червоні шапочки». Квіти рожеві або білуваті. Догляд: помірне світло (мутанти потребують захисту від прямого сонця), полив рідко. Gymnocalycium — «гола чашечка».",
                imageUrl: "entities/plants/gymnocalycium_mihanovichii.png",
                imageIsometricUrl: "entities/plants/gymnocalycium_mihanovichii_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.08
                soilFormulaId: arid),

            Plant.Create(
                name: "Астрофітум зірчастий",
                nameLatin: "Astrophytum asterias",
                description: "Кактус з Мексики та США без колючок. За формою нагадує морську зірку або сплюснутий гарбуз. Ребра низькі, округлі. Епідерміс сіро-зелений з білими крапками. Квіти жовті з червоною серединкою. Дуже повільний ріст. Занесений до Червоної книги. Догляд: яскраве світло, мінімум води. Astrophytum — «зіркова рослина».",
                imageUrl: "entities/plants/astrophytum_asterias.png",
                imageIsometricUrl: "entities/plants/astrophytum_asterias_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.07
                soilFormulaId: arid),

            Plant.Create(
                name: "Ребуція крихітна",
                nameLatin: "Rebutia minuscula",
                description: "Маленький кулястий кактус з Болівії та Аргентини. Стебло до 5 см, вкрите білими або золотистими колючками. Навесні рясно вкривається великими яскраво-помаранчевими або червоними квітами — діаметр квітки може перевищувати діаметр стебла. Догляд: яскраве світло, полив помірний у вегетацію. Ідеальний для міні-флораріумів через компактність та яскравість квітіння.",
                imageUrl: "entities/plants/rebutia_minuscula.png",
                imageIsometricUrl: "entities/plants/rebutia_minuscula_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Small, // 0.06
                soilFormulaId: arid),

            Plant.Create(
                name: "Пародія Магніфіка",
                nameLatin: "Parodia magnifica",
                description: "Кактус з Бразилії. Спочатку кулястий, з часом стає циліндричним. Відрізняється сизо-блакитним епідермісом та тонкими золотистими колючками на чітких ребрах. Квіти жовті, великі. Догляд: яскраве світло, полив рідко. Parodia названа на честь італійського ботаніка Лоренцо Пароді. Magnifica — «чудова». Рідкісний у природі через руйнування середовища.",
                imageUrl: "entities/plants/parodia_magnifica.png",
                imageIsometricUrl: "entities/plants/parodia_magnifica_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.12
                soilFormulaId: arid),

            Plant.Create(
                name: "Клейстокактус Штрауса",
                nameLatin: "Cleistocactus strausii",
                description: "Стовпчастий кактус з Болівії та Аргентини. Стебло до 1 м заввишки, повністю вкрите білими тонкими колючками, що нагадують шерсть або сіно. Квіти червоні, трубчасті. Додає вертикальний акцент у композицію. Догляд: максимум світла, полив помірний. Cleistocactus — «закритий кактус» (квіти майже не розкриваються). Використовується як «свічка» у сукулентних аранжуваннях.",
                imageUrl: "entities/plants/cleistocactus_strausii.png",
                imageIsometricUrl: "entities/plants/cleistocactus_strausii_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Large, // 0.20
                soilFormulaId: arid),

            Plant.Create(
                name: "Цереус Перуанський Монстрозус",
                nameLatin: "Cereus peruvianus f. monstrosus",
                description: "Скеляста (монстрозна) форма перуанського цереуса. Стебло з хаотичними розгалуженнями, горбками та виростами — кожна рослина має унікальну химерну форму, схожу на мініатюрні гори або корали. Колір сіро-зелений. Догляд: яскраве світло, полив рідко. Монстрозна мутація культивується з XIX століття. Cereus — «свічка» з латини. Ідеальний фокусний елемент для пустельного флораріуму.",
                imageUrl: "entities/plants/cereus_peruvianus_f_monstrosus.png",
                imageIsometricUrl: "entities/plants/cereus_peruvianus_f_monstrosus_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Large, // 0.18
                soilFormulaId: arid),

            Plant.Create(
                name: "Ферокактус ширококолючковий",
                nameLatin: "Ferocactus latispinus",
                description: "Кактус з Мексики з дуже характерними широкими, пласкими та загнутими колючками червонуватого або жовтого кольору. Стебло кулясте, ребристе. Виглядає агресивно та декоративно. Квіти рожеві або пурпурні. Dogляд: яскраве світло, полив рідко. Ferocactus — «дикий кактус» (від лат. ferox). Latispinus — «ширококолючковий».",
                imageUrl: "entities/plants/ferocactus_latispinus.png",
                imageIsometricUrl: "entities/plants/ferocactus_latispinus_isometric.png",
                category: PlantCategory.Cacti,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryLow,
                humidityLevel: PlantHumidityLevel.VeryLow,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: arid),

            // ===== ТРОПІЧНІ =====
            Plant.Create(
                name: "Фітонія Вершаффельта",
                nameLatin: "Fittonia albivenis",
                description: "Низькоросла трав'яниста рослина з родини акантових (Acanthaceae), походить з тропічних лісів Перу та Колумбії. Овальне листя 5–8 см вкрите мережею яскраво-рожевих, білих або червоних жилок на темно-зеленому фоні — нагадує нервову систему. Стебла повзучі, м'ясисті. Формує щільні килимки заввишки до 15 см. Догляд: тінь або півтінь, висока вологість (ідеально для закритих флораріумів), регулярний полив. Назва на честь сестер Фіттон — авторок ботанічного атласу XIX століття.",
                imageUrl: "entities/plants/fittonia_albivenis.png",
                imageIsometricUrl: "entities/plants/fittonia_albivenis_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.Low,
                waterNeed: PlantWaterNeed.High,
                humidityLevel: PlantHumidityLevel.High,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: tropical),

            Plant.Create(
                name: "Солейролія (Сльози дитини)",
                nameLatin: "Soleirolia soleirolii",
                description: "Трав'яниста рослина з Корсики та Сардинії. Створює щільний зелений килим з мікроскопічних листочків (3–5 мм) на тонких ниткоподібних стеблах. Ріст швидкий, покриває всю поверхню. Любить постійну легку вологість ґрунту. Dogляд: півтінь, регулярний полив, можна обприскувати. Використовується як «газон» у терраріумах та флораріумах. Ідеальна для закритих екосистем.",
                imageUrl: "entities/plants/soleirolia_soleirolii.png",
                imageIsometricUrl: "entities/plants/soleirolia_soleirolii_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.High,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.Small, // 0.05
                soilFormulaId: tropical),

            Plant.Create(
                name: "Пілея обгорнута",
                nameLatin: "Pilea involucrata",
                description: "Компактна рослина з родини кропивових, родом з Центральної та Південної Америки. Має сильно текстуроване листя — поверхня з горбками та заглибленнями. Колір коричнево-зелений з бронзовим відтінком. Листя овальне, до 7 см. Догляд: півтінь, помірна вологість, регулярний полив. Pilea — «капелюх» (за формою плодів). Використовується в тропічних флораріумах як текстурований акцент.",
                imageUrl: "entities/plants/pilea_involucrata.png",
                imageIsometricUrl: "entities/plants/pilea_involucrata_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.Medium,
                humidityLevel: PlantHumidityLevel.High,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: tropical),

            Plant.Create(
                name: "Традесканція Зебрина",
                nameLatin: "Tradescantia zebrina",
                description: "Повзуча рослина з Мексики з сріблясто-фіолетовим блискучим листям. Листя овальне, загострене, з двома сріблястими смугами на фіолетовому фоні. Нижня сторона листа пурпурна. Швидко росте, створює колірний акцент. Dogляд: помірне світло, регулярний полив. Невибаглива, ідеальна для початківців. Добре виглядає в ампельних композиціях.",
                imageUrl: "entities/plants/tradescantia_zebrina.png",
                imageIsometricUrl: "entities/plants/tradescantia_zebrina_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.Medium,
                humidityLevel: PlantHumidityLevel.Medium,
                itemMaxSize: ProjectItemSize.Large, // 0.20
                soilFormulaId: hardy),

            Plant.Create(
                name: "Аглаонема Марія",
                nameLatin: "Aglaonema Maria",
                description: "Тіньовитривала рослина з родини ароїдних, родом з тропіків Південно-Східної Азії. Листя еліптичне, темно-зелене з сріблясто-зеленими плямами вздовж центральної жилки. Росте повільно в обмеженому об'ємі — ідеально для флораріумів. Догляд: тінь, помірна вологість, полив коли верхній шар ґрунту просохне. Токсична при вживанні. Один з найпопулярніших офісних рослин.",
                imageUrl: "entities/plants/aglaonema_maria.png",
                imageIsometricUrl: "entities/plants/aglaonema_maria_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.Low,
                waterNeed: PlantWaterNeed.Medium,
                humidityLevel: PlantHumidityLevel.High,
                itemMaxSize: ProjectItemSize.Large, // 0.25
                soilFormulaId: begonia),

            Plant.Create(
                name: "Сингоніум Піксі",
                nameLatin: "Syngonium podophyllum Pixie",
                description: "Мініатюрна ліана з Центральної Америки. Листя стрілоподібне, біло-зелене з рожевими прожилками. Сорт Pixie — компактний, листя менше 10 см. Потребує регулярної обрізки для збереження компактності. Dogляд: півтінь, висока вологість, регулярний полив. Syngonium — «з'єднані зародки». Токсична. Декоративна завдяки контрастному забарвленню.",
                imageUrl: "entities/plants/syngonium_podophyllum_pixie.png",
                imageIsometricUrl: "entities/plants/syngonium_podophyllum_pixie_isometric.png",
                category: PlantCategory.Tropical,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.High,
                humidityLevel: PlantHumidityLevel.High,
                itemMaxSize: ProjectItemSize.Medium, // 0.15
                soilFormulaId: aroid),

            // ===== ПАПОРОТІ =====
            Plant.Create(
                name: "Адіантум (Венерине волосся)",
                nameLatin: "Adiantum raddianum",
                description: "Декоративна папороть з родини птерисових (Pteridaceae), походить з тропіків Південної Америки. Вайї ніжні, багаторазово перисті, на тонких чорно-коричневих черешках. Сегменти клиноподібні, світло-зелені, напівпрозорі. Загальна форма нагадує трикутник або веер. Висота до 25–40 см. Догляд: тінь, висока вологість, постійна вологість ґрунту. Adiantum — «незмочуваний» (крапельки води зісковзують з листя). Чутлива до сухого повітря та прямих сонячних променів.",
                imageUrl: "entities/plants/adiantum_raddianum.png",
                imageIsometricUrl: "entities/plants/adiantum_raddianum_isometric.png",
                category: PlantCategory.Ferns,
                lightLevel: PlantLightLevel.Low,
                waterNeed: PlantWaterNeed.VeryHigh,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.Large, // 0.25
                soilFormulaId: fern),

            Plant.Create(
                name: "Нефролепіс піднесений",
                nameLatin: "Nephrolepis exaltata",
                description: "Класична кімнатна папороть з тропіків усіх континентів. Вайї перисті, дуговидно зігнуті, світло-зелені. Формує щільну «фонтанну» крону. Догляд: півтінь, висока вологість, регулярний полив. Nephrolepis — «ниркоподібна луска». Використовується для очищення повітря. Одна з перших папоротей, введених у культуру у XIX столітті.",
                imageUrl: "entities/plants/nephrolepis_exaltata.png",
                imageIsometricUrl: "entities/plants/nephrolepis_exaltata_isometric.png",
                category: PlantCategory.Ferns,
                lightLevel: PlantLightLevel.Low,
                waterNeed: PlantWaterNeed.VeryHigh,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.ExtraLarge, // 0.30
                soilFormulaId: fern),

            // ===== МОХИ =====
            Plant.Create(
                name: "Мох Леукобрій (Подушковий)",
                nameLatin: "Leucobryum glaucum",
                description: "Мох з родини дікранових (Dicranaceae), поширений у помірних та холодних регіонах Північної півкулі. Стебла до 5 см, листки ланцетні, сріблясто-зелені з блакитним відтінком. Формує щільні подушкоподібні дернини. Використовується в терраріумах для імітації луговини або лісової підстилки. Догляд: висока вологість, розсіяне світло, постійна легка вологість. Leucobryum — «білий мох». Класика для мосаріумів.",
                imageUrl: "entities/plants/leucobryum_glaucum.png",
                imageIsometricUrl: "entities/plants/leucobryum_glaucum_isometric.png",
                category: PlantCategory.Mosses,
                lightLevel: PlantLightLevel.Low,
                waterNeed: PlantWaterNeed.High,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.Small, // 0.05
                soilFormulaId: forest),

            Plant.Create(
                name: "Яванський мох",
                nameLatin: "Taxiphyllum barbieri",
                description: "Мох з Південно-Східної Азії. Може рости як у воді (популярний у акваріумах), так і на дуже вологому ґрунті. Гілки ниткоподібні, листки дрібні, світло-зелені. Створює ефект джунглів або «бороди». Швидко розростається. Догляд: висока вологість, півтінь. Використовується в акваріумістиці з 1970-х. В терраріумах створює м'який килим на камінні та корі.",
                imageUrl: "entities/plants/taxiphyllum_barbieri.png",
                imageIsometricUrl: "entities/plants/taxiphyllum_barbieri_isometric.png",
                category: PlantCategory.Mosses,
                lightLevel: PlantLightLevel.Medium,
                waterNeed: PlantWaterNeed.VeryHigh,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.Small, // 0.03
                soilFormulaId: forest),

            // ===== ХИЖІ РОСЛИНИ =====
            Plant.Create(
                name: "Венерина Мухоловка",
                nameLatin: "Dionaea muscipula",
                description: "Хижа рослина з родини росичкових (Droseraceae), ендемік болотистих саван Південної Кароліни (США). Листя зібране в прикореневу розетку; кожен лист має дві лопаті-«челюсті» з тригерами та травними ферментами. Пастки закриваються за 0,1 секунди при подразненні. Dogляд: яскраве світло, постійна вологість (краще дистильована вода), кислий торф. Charles Darwin називав її «найдивовижнішою рослиною у світі». Захищена у природі.",
                imageUrl: "entities/plants/dionaea_muscipula.png",
                imageIsometricUrl: "entities/plants/dionaea_muscipula_isometric.png",
                category: PlantCategory.CarnivorousPlants,
                lightLevel: PlantLightLevel.High,
                waterNeed: PlantWaterNeed.VeryHigh,
                humidityLevel: PlantHumidityLevel.High,
                itemMaxSize: ProjectItemSize.Medium, // 0.13
                soilFormulaId: carniv),

            Plant.Create(
                name: "Росичка Капська",
                nameLatin: "Drosera capensis",
                description: "Хижа рослина з ПАР. Листя вузьке, довге, вкрите липкими краплями (муцилом), що світяться на сонці як роса. Комахи прилипають, листя поступово згортається. Квіти рожеві, на високому квітконосі. Ефективно ловить дрібних комах. Dogляд: яскраве світло, постійна вологість, кислий торф без добрив. Drosera — «росяна» з грецької. Один з найпростіших хижих рослин для вирощування.",
                imageUrl: "entities/plants/drosera_capensis.png",
                imageIsometricUrl: "entities/plants/drosera_capensis_isometric.png",
                category: PlantCategory.CarnivorousPlants,
                lightLevel: PlantLightLevel.VeryHigh,
                waterNeed: PlantWaterNeed.VeryHigh,
                humidityLevel: PlantHumidityLevel.VeryHigh,
                itemMaxSize: ProjectItemSize.Medium, // 0.10
                soilFormulaId: carniv),
        };

        foreach (var result in plants)
        {
            if (result.IsSuccess)
            {
                context.Set<Plant>().Add(result.Value);
            }
        }

        context.SaveChanges();
    }
    public static void SeedSoilTypes(DbContext context)
    {
        if (context.Set<SoilType>().Any())
        {
            return;
        }

        var soilTypes = new[]
        {
            SoilType.Create("Кокосовий торф"),           // 1
            SoilType.Create("Сфагнум"),                   // 2
            SoilType.Create("Перліт"),                    // 3
            SoilType.Create("Деревне вугілля"),           // 4
            SoilType.Create("Дренаж"),                    // 5
            SoilType.Create("Грунт для кактусів"),        // 6
            SoilType.Create("Кварцовий пісок"),           // 7
            SoilType.Create("Дрібний гравій"),            // 8
            SoilType.Create("Лісова земля (кисла)"),      // 9
            SoilType.Create("Подрібнений мох сфагнум"),   // 10
            SoilType.Create("Вермикуліт"),                // 11
            SoilType.Create("Вугілля"),                   // 12
            SoilType.Create("Верховий кислий торф"),      // 13
            SoilType.Create("Перліт або чистий кварцовий пісок"), // 14
            SoilType.Create("Соснова кора (дрібна)"),    // 15
            SoilType.Create("Кокосові чіпси"),            // 16
            SoilType.Create("Цеоліт/Лечуза Пон"),         // 17
            SoilType.Create("Лава"),                      // 18
            SoilType.Create("Пісок"),                     // 19
            SoilType.Create("Листова земля"),             // 20
            SoilType.Create("Торф"),                      // 21
            SoilType.Create("Хвойний опад"),              // 22
            SoilType.Create("Кокосовий субстрат"),        // 23
            SoilType.Create("Різаний сфагнум"),           // 24
            SoilType.Create("Біогумус"),                  // 25
            SoilType.Create("Академама (обпалена глина)"), // 26
            SoilType.Create("Гумус"),                     // 27
            SoilType.Create("Річковий пісок"),            // 28
            SoilType.Create("Універсальний садовий ґрунт"), // 29
            SoilType.Create("Дренажний шар (на дно)"),    // 30
            SoilType.Create("Садова земля"),              // 31
            SoilType.Create("Кора"),                      // 32
        };

        foreach (var result in soilTypes)
        {
            if (result.IsSuccess)
            {
                context.Set<SoilType>().Add(result.Value);
            }
        }

        context.SaveChanges();
    }

    public static void SeedSoilFormulas(DbContext context)
    {
        if (context.Set<SoilFormulas>().Any())
        {
            return;
        }

        // Load persisted SoilTypes to resolve IDs by name
        var types = context.Set<SoilType>()
            .ToDictionary(t => t.Name, t => t.Id);

        int Id(string name) => types[name];

        var formulas = new (string name, List<SoilFormulaItem> items)[]
        {
            (
                "Тропічний вологий (Класика)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Кокосовий торф"),     40, 0).Value,
                    SoilFormulaItem.Create(Id("Сфагнум"),             20, 1).Value,
                    SoilFormulaItem.Create(Id("Перліт"),              20, 2).Value,
                    SoilFormulaItem.Create(Id("Деревне вугілля"),     10, 3).Value,
                    SoilFormulaItem.Create(Id("Дренаж"),              10, 4).Value,
                }
            ),
            (
                "Пустельний мікс (Сукулент)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Грунт для кактусів"),  40, 0).Value,
                    SoilFormulaItem.Create(Id("Кварцовий пісок"),     30, 1).Value,
                    SoilFormulaItem.Create(Id("Дрібний гравій"),      20, 2).Value,
                    SoilFormulaItem.Create(Id("Дренаж"),              10, 3).Value,
                }
            ),
            (
                "Лісовий мох (Мосаріум)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Лісова земля (кисла)"),     40, 0).Value,
                    SoilFormulaItem.Create(Id("Подрібнений мох сфагнум"),  30, 1).Value,
                    SoilFormulaItem.Create(Id("Вермикуліт"),               20, 2).Value,
                    SoilFormulaItem.Create(Id("Вугілля"),                  10, 3).Value,
                }
            ),
            (
                "Хижий (Для венериної мухоловки)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Верховий кислий торф"),                  50, 0).Value,
                    SoilFormulaItem.Create(Id("Перліт або чистий кварцовий пісок"),     50, 1).Value,
                }
            ),
            (
                "Епіфітний (Орхідейний)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Соснова кора (дрібна)"),  60, 0).Value,
                    SoilFormulaItem.Create(Id("Сфагнум"),                20, 1).Value,
                    SoilFormulaItem.Create(Id("Кокосові чіпси"),         10, 2).Value,
                    SoilFormulaItem.Create(Id("Деревне вугілля"),        10, 3).Value,
                }
            ),
            (
                "Аридний мінеральний",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Цеоліт/Лечуза Пон"),  60, 0).Value,
                    SoilFormulaItem.Create(Id("Лава"),                20, 1).Value,
                    SoilFormulaItem.Create(Id("Пісок"),               20, 2).Value,
                }
            ),
            (
                "Папоротевий легкий",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Листова земля"),   40, 0).Value,
                    SoilFormulaItem.Create(Id("Торф"),             30, 1).Value,
                    SoilFormulaItem.Create(Id("Перліт"),           20, 2).Value,
                    SoilFormulaItem.Create(Id("Хвойний опад"),     10, 3).Value,
                }
            ),
            (
                "Бегонієвий (Пухкий)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Кокосовий субстрат"),  50, 0).Value,
                    SoilFormulaItem.Create(Id("Різаний сфагнум"),      20, 1).Value,
                    SoilFormulaItem.Create(Id("Вермикуліт"),           20, 2).Value,
                    SoilFormulaItem.Create(Id("Біогумус"),             10, 3).Value,
                }
            ),
            (
                "Бонсай-мікс",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Академама (обпалена глина)"),  30, 0).Value,
                    SoilFormulaItem.Create(Id("Гумус"),                        40, 1).Value,
                    SoilFormulaItem.Create(Id("Річковий пісок"),               30, 2).Value,
                }
            ),
            (
                "Універсальний Hardy",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Універсальний садовий ґрунт"),  60, 0).Value,
                    SoilFormulaItem.Create(Id("Перліт"),                        20, 1).Value,
                    SoilFormulaItem.Create(Id("Дренажний шар (на дно)"),        20, 2).Value,
                }
            ),
            (
                "Ароїдний (Aroid Mix)",
                new List<SoilFormulaItem>
                {
                    SoilFormulaItem.Create(Id("Садова земля"),     30, 0).Value,
                    SoilFormulaItem.Create(Id("Кора"),             30, 1).Value,
                    SoilFormulaItem.Create(Id("Кокосові чіпси"),   20, 2).Value,
                    SoilFormulaItem.Create(Id("Перліт"),           10, 3).Value,
                    SoilFormulaItem.Create(Id("Вугілля"),          10, 4).Value,
                }
            ),
        };

        foreach (var (name, items) in formulas)
        {
            var result = SoilFormulas.Create(name, items);
            if (result.IsSuccess)
            {
                context.Set<SoilFormulas>().Add(result.Value);
            }
        }

        context.SaveChanges();
    }
    public static void SeedDecorations(DbContext context)
    {
        if (context.Set<Decoration>().Any())
        {
            return;
        }

        var decorations = new[]
        {
            // Каміння
            Decoration.Create(
                name: "Біла морська галька",
                description: "Гладеньке біле каміння середнього розміру. Створює яскравий контраст із зеленим мохом.",
                category: DecorationCategory.Stones,
                imageUrl: "entities/decorations/white_sea_gravel.png",
                imageIsometricUrl: "entities/decorations/white_sea_gravel_isometric.png"),

            Decoration.Create(
                name: "Чорна вулканічна лава",
                description: "Пористе чорне каміння з грубою текстурою. Ідеально підходить для пустельних композицій.",
                category: DecorationCategory.Stones,
                imageUrl: "entities/decorations/volcanic_lava_rock.png",
                imageIsometricUrl: "entities/decorations/volcanic_lava_rock_isometric.png"),

            Decoration.Create(
                name: "Сланцева крихта",
                description: "Пласкі шматочки сірого сланцю. Можна використовувати для створення імітації скель або східців.",
                category: DecorationCategory.Stones,
                imageUrl: "entities/decorations/slate_chips.png",
                imageIsometricUrl: "entities/decorations/slate_chips_isometric.png"),

            Decoration.Create(
                name: "Червона яшма",
                description: "Природний мінерал насиченого теракотового кольору. Додає теплих відтінків флораріуму.",
                category: DecorationCategory.Stones,
                imageUrl: "entities/decorations/red_jasper.png",
                imageIsometricUrl: "entities/decorations/red_jasper_isometric.png"),

            // Пісок
            Decoration.Create(
                name: "Блакитний кварцовий пісок",
                description: "Дрібнозернистий пісок насиченого кольору. Використовується для імітації води або річок.",
                category: DecorationCategory.Sand,
                imageUrl: "entities/decorations/blue_quartz_sand.png",
                imageIsometricUrl: "entities/decorations/blue_quartz_sand_isometric.png"),

            Decoration.Create(
                name: "Золотистий пустельний пісок",
                description: "Натуральний чистий пісок для створення реалістичних пустельних пейзажів.",
                category: DecorationCategory.Sand,
                imageUrl: "entities/decorations/desert_gold_sand.png",
                imageIsometricUrl: "entities/decorations/desert_gold_sand_isometric.png"),

            // Дерево
            Decoration.Create(
                name: "Дубова коряга \"Дрифтвуд\"",
                description: "Вивітрена водою деревина вигадливої форми. Нагадує старе дерево в мініатюрі.",
                category: DecorationCategory.Wood,
                imageUrl: "entities/decorations/oak_driftwood.png",
                imageIsometricUrl: "entities/decorations/oak_driftwood_isometric.png"),

            Decoration.Create(
                name: "Кора соснова",
                description: "Натуральні шматочки кори для декорування поверхні ґрунту в лісових композиціях.",
                category: DecorationCategory.Wood,
                imageUrl: "entities/decorations/pine_bark.png",
                imageIsometricUrl: "entities/decorations/pine_bark_isometric.png"),

            // Фігурки
            Decoration.Create(
                name: "Класичний маяк",
                description: "Деталізована фігурка біло-червоного маяка. Центр композиції для морської тематики.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/classic_lighthouse.png",
                imageIsometricUrl: "entities/decorations/classic_lighthouse_isometric.png"),

            Decoration.Create(
                name: "Японська брама Торії",
                description: "Червона ритуальна брама. Додає східного колориту та спокою вашому садочку.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/torii_gate.png",
                imageIsometricUrl: "entities/decorations/torii_gate_isometric.png"),

            Decoration.Create(
                name: "Кам'яний місток",
                description: "Маленький вигнутий місток, що ідеально лягає над \"річкою\" з піску.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/stone_bridge.png",
                imageIsometricUrl: "entities/decorations/stone_bridge_isometric.png"),

            Decoration.Create(
                name: "Будиночок хобіта",
                description: "Крихітні круглі двері в \"пагорбі\", прикрашені імітацією ліан.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/hobbit_house.png",
                imageIsometricUrl: "entities/decorations/hobbit_house_isometric.png"),

            Decoration.Create(
                name: "Мініатюрна лава",
                description: "Дерев'яна садова лавка для створення затишного паркового куточка.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/mini_bench.png",
                imageIsometricUrl: "entities/decorations/mini_bench_isometric.png"),

            Decoration.Create(
                name: "Набір лісових грибів",
                description: "Три червоних мухомори різного розміру на спільній основі.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/forest_mushrooms.png",
                imageIsometricUrl: "entities/decorations/forest_mushrooms_isometric.png"),

            Decoration.Create(
                name: "Спляче лисеня",
                description: "Маленька помаранчева фігурка лисиці, що згорнулася клубочком.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/sleeping_fox.png",
                imageIsometricUrl: "entities/decorations/sleeping_fox_isometric.png"),

            Decoration.Create(
                name: "Керамічна панда",
                description: "Маленька панда, що жує бамбук. Добре виглядає поруч із сукулентами.",
                category: DecorationCategory.Figurines,
                imageUrl: "entities/decorations/panda_figure.png",
                imageIsometricUrl: "entities/decorations/panda_figure_isometric.png"),

            // Природа
            Decoration.Create(
                name: "Морська мушля \"Рапана\"",
                description: "Натуральна мушля невеликого розміру для акцентів у відкритих формах.",
                category: DecorationCategory.Nature,
                imageUrl: "entities/decorations/sea_shell.png",
                imageIsometricUrl: "entities/decorations/sea_shell_isometric.png"),

            // Мінерали
            Decoration.Create(
                name: "Друза аметисту",
                description: "Натуральний фіолетовий кристал. Додає магічного вигляду та блиску під світлом.",
                category: DecorationCategory.Minerals,
                imageUrl: "entities/decorations/amethyst_cluster.png",
                imageIsometricUrl: "entities/decorations/amethyst_cluster_isometric.png"),

            Decoration.Create(
                name: "Прозорий гірський кришталь",
                description: "Вертикальний гострий кристал, що імітує крижану скелю.",
                category: DecorationCategory.Minerals,
                imageUrl: "entities/decorations/quartz_crystal.png",
                imageIsometricUrl: "entities/decorations/quartz_crystal_isometric.png"),

            Decoration.Create(
                name: "Бурштинова крихта",
                description: "Дрібні прозорі камінці медового кольору для розсипання по ґрунту.",
                category: DecorationCategory.Minerals,
                imageUrl: "entities/decorations/amber_chips.png",
                imageIsometricUrl: "entities/decorations/amber_chips_isometric.png"),
        };

        foreach (var result in decorations)
        {
            if (result.IsSuccess)
            {
                context.Set<Decoration>().Add(result.Value);
            }
        }

        context.SaveChanges();
    }

    public static void SeedContainers(DbContext context)
    {
        if (context.Set<Container>().Any())
        {
            return;
        }

        var containers = new[]
        {
            Container.Create(
                name: "Ікосаедр (Геометрія)",
                description: "Класична багатогранна форма з 20 трикутних граней. Ідеальна для сукулентних композицій.",
                volume: 2.5f,
                isClosed: false,
                imageUrl: "entities/containers/icosahedron.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Флораріум \"Крапля\"",
                description: "Витончена обтічна форма з круглим отвором. Може бути як настільною, так і підвісною.",
                volume: 1.8f,
                isClosed: false,
                imageUrl: "entities/containers/teardrop.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Мінімалістичний Куб",
                description: "Проста та зрозуміла форма для створення сучасних міні-ландшафтів з чіткими лініями.",
                volume: 3.0f,
                isClosed: false,
                imageUrl: "entities/containers/cube.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Еко-Сфера (Закрита)",
                description: "Кругла важа з герметичною корковою кришкою. Створює замкнений цикл для вологолюбних рослин.",
                volume: 3.5f,
                isClosed: true,
                imageUrl: "entities/containers/sphere_closed.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Додекаедр XL",
                description: "Велика дванадцятигранна форма. Завдяки широким граням зручно розташовувати великі камені.",
                volume: 4.2f,
                isClosed: false,
                imageUrl: "entities/containers/dodecahedron.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Велика Піраміда",
                description: "Форма з високим шпилем, що ідеально підходить для центральних високих акцентів (напр. кактусів).",
                volume: 2.2f,
                isClosed: false,
                imageUrl: "entities/containers/pyramid.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Призма \"Кристал\"",
                description: "Видовжена вертикальна форма, що нагадує кристал мінералу. Виглядає футуристично.",
                volume: 1.5f,
                isClosed: false,
                imageUrl: "entities/containers/prism.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Чаша \"Лотос\"",
                description: "Широка відкрита чаша з багатьма дрібними гранями. Найкращий вибір для саду мохів.",
                volume: 2.8f,
                isClosed: false,
                imageUrl: "entities/containers/lotus_bowl.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Лабораторний Циліндр",
                description: "Висока скляна колба з кришкою. Для створення вертикальних лісових композицій.",
                volume: 5.0f,
                isClosed: true,
                imageUrl: "entities/containers/cylinder.png",
                imageIsometricUrl: null),

            Container.Create(
                name: "Пісочний годинник",
                description: "Незвична форма з вузькою талією, що розділяє два рівні декору (наприклад, каміння та рослини).",
                volume: 3.2f,
                isClosed: false,
                imageUrl: "entities/containers/hourglass.png",
                imageIsometricUrl: null),
        };

        foreach (var result in containers)
        {
            if (result.IsSuccess)
            {
                context.Set<Container>().Add(result.Value);
            }
        }

        context.SaveChanges();
    }
}