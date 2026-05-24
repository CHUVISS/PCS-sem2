using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using TouristGuide.Models;

namespace TouristGuide.Data;

public class AppDbContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    public DbSet<Attraction> Attractions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(folder, "TouristGuide", "tourist_guide_v2.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Region).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Attraction>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired().HasMaxLength(200);
            e.HasOne(a => a.City)
             .WithMany(c => c.Attractions)
             .HasForeignKey(a => a.CityId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        modelBuilder.Entity<City>().HasData(
            new City
            {
                Id = 1,
                Name = "Москва",
                Region = "Центральный федеральный округ",
                Population = 13010112,
                History = "Москва — столица и крупнейший город России. Впервые упоминается в летописях в 1147 году. На протяжении веков город служил резиденцией московских царей и российских императоров, был духовным и культурным центром страны. Сегодня Москва — мегаполис мирового значения с богатейшим историческим наследием.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/MSK_kolaj.jpg/800px-MSK_kolaj.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/14/Coat_of_Arms_of_Moscow.svg/200px-Coat_of_Arms_of_Moscow.svg.png"
            },
            new City
            {
                Id = 2,
                Name = "Санкт-Петербург",
                Region = "Северо-Западный федеральный округ",
                Population = 5601000,
                History = "Санкт-Петербург основан Петром I в 1703 году как «окно в Европу». Город был столицей Российской империи с 1712 по 1918 год. Известен своей уникальной архитектурой, многочисленными музеями и белыми ночами. ЮНЕСКО признало исторический центр города объектом Всемирного наследия.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Saint_Petersburg_collage.jpg/800px-Saint_Petersburg_collage.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/ce/Coat_of_arms_of_Saint_Petersburg_%282003%29.svg/200px-Coat_of_arms_of_Saint_Petersburg_%282003%29.svg.png"
            },
            new City
            {
                Id = 3,
                Name = "Казань",
                Region = "Приволжский федеральный округ",
                Population = 1308660,
                History = "Казань — столица Республики Татарстан, один из крупнейших культурных, экономических и исторических центров России. Основана в XIV веке. В 2015 году принимала Чемпионат мира по водным видам спорта, а в 2018 году — матчи Чемпионата мира по футболу. Уникальное сочетание русской и татарской культур.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/84/Kazan_Kremlin_2019.jpg/800px-Kazan_Kremlin_2019.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/17/Coat_of_Arms_of_Kazan.svg/200px-Coat_of_Arms_of_Kazan.svg.png"
            },
            new City
            {
                Id = 4,
                Name = "Новосибирск",
                Region = "Сибирский федеральный округ",
                Population = 1625631,
                History = "Новосибирск — третий по численности населения город России и крупнейший город Сибири. Основан в 1893 году как посёлок при строительстве железнодорожного моста через Обь. Стремительный рост позволил ему стать крупным научным, культурным и экономическим центром. Здесь находится знаменитый Академгородок — уникальный научный центр мирового значения.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/26/Novosibirsk_theatre.jpg/800px-Novosibirsk_theatre.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f8/Coat_of_arms_of_Novosibirsk.svg/200px-Coat_of_arms_of_Novosibirsk.svg.png"
            },
            new City
            {
                Id = 5,
                Name = "Екатеринбург",
                Region = "Уральский федеральный округ",
                Population = 1544376,
                History = "Екатеринбург — четвёртый по численности населения город России, столица Уральского федерального округа. Основан в 1723 году при Петре I. Город сыграл ключевую роль в освоении Урала. Здесь в 1918 году была расстреляна семья последнего российского императора Николая II. Сегодня — крупный промышленный и культурный центр страны.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9c/Yekaterinburg_Montage_2018.jpg/800px-Yekaterinburg_Montage_2018.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/55/Coat_of_Arms_of_Yekaterinburg_%282008%29.svg/200px-Coat_of_Arms_of_Yekaterinburg_%282008%29.svg.png"
            },
            new City
            {
                Id = 6,
                Name = "Нижний Новгород",
                Region = "Приволжский федеральный округ",
                Population = 1228198,
                History = "Нижний Новгород основан в 1221 году великим князем Юрием Всеволодовичем у слияния Волги и Оки. Долгое время был важнейшим торговым центром России — знаменитая Нижегородская ярмарка считалась крупнейшей в Европе. В советское время носил название Горький. Сегодня это крупный промышленный и культурный центр Поволжья.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3b/Nizhny_novgorod_kremlin1.jpg/800px-Nizhny_novgorod_kremlin1.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/29/Coat_of_Arms_of_Nizhny_Novgorod.svg/200px-Coat_of_Arms_of_Nizhny_Novgorod.svg.png"
            },
            new City
            {
                Id = 7,
                Name = "Сочи",
                Region = "Краснодарский край",
                Population = 450000,
                History = "Сочи — главный курортный город России, «летняя столица» страны. Расположен на черноморском побережье у подножия Кавказских гор. В 2014 году принимал зимние Олимпийские игры. Город известен субтропическим климатом, уникальным сочетанием морских пляжей и горных пейзажей, а также знаменитыми санаториями и горнолыжными курортами.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/Sochi_from_above.jpg/800px-Sochi_from_above.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4f/Coat_of_Arms_of_Sochi.svg/200px-Coat_of_Arms_of_Sochi.svg.png"
            },
            new City
            {
                Id = 8,
                Name = "Владивосток",
                Region = "Дальневосточный федеральный округ",
                Population = 600378,
                History = "Владивосток основан в 1860 году как военный пост на берегу бухты Золотой Рог. Сегодня это главный тихоокеанский порт России и ворота страны в Азиатско-Тихоокеанский регион. В 2012 году здесь прошёл саммит АТЭС, были построены уникальные вантовые мосты. Город расположен на живописных сопках с видом на Японское море.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/76/Vladivostok_montage.jpg/800px-Vladivostok_montage.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Coat_of_Arms_of_Vladivostok.svg/200px-Coat_of_Arms_of_Vladivostok.svg.png"
            },
            new City
            {
                Id = 9,
                Name = "Великий Новгород",
                Region = "Северо-Западный федеральный округ",
                Population = 222000,
                History = "Великий Новгород — один из древнейших городов России, колыбель русской государственности. Основан в IX веке, в 862 году здесь начал княжить Рюрик. Новгородская республика была одним из крупнейших государств средневековой Европы. Кремль и архитектурные памятники города внесены в список Всемирного наследия ЮНЕСКО.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b0/Novgorod-kremlin.jpg/800px-Novgorod-kremlin.jpg",
                CoatOfArmsUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/46/Coat_of_Arms_of_Veliky_Novgorod.svg/200px-Coat_of_Arms_of_Veliky_Novgorod.svg.png"
            }
        );

        modelBuilder.Entity<Attraction>().HasData(
            new Attraction
            {
                Id = 1,
                CityId = 1,
                Name = "Красная площадь",
                Description = "Главная площадь Москвы и всей России, объект Всемирного наследия ЮНЕСКО.",
                History = "Красная площадь существует с конца XV века. Название означает «красивая площадь» в старославянском. Здесь расположены Кремль, Собор Василия Блаженного, Мавзолей Ленина и ГУМ. Площадь была местом официальных церемоний, народных гуляний и военных парадов.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Red_Square_and_the_Kremlin_from_Zaryadye_Park.jpg/800px-Red_Square_and_the_Kremlin_from_Zaryadye_Park.jpg",
                WorkingHours = "Круглосуточно (открытая площадь)",
                TicketPrice = null
            },
            new Attraction
            {
                Id = 2,
                CityId = 1,
                Name = "Московский Кремль",
                Description = "Древняя крепость в центре Москвы — официальная резиденция Президента РФ.",
                History = "Кремль был основан в 1156 году князем Юрием Долгоруким. Современные кирпичные стены были построены в 1485–1495 годах. Комплекс включает соборы, дворцы, башни и музеи. Является объектом Всемирного наследия ЮНЕСКО.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Vodovzvodnaya_Tower_of_the_Moscow_Kremlin.jpg/800px-Vodovzvodnaya_Tower_of_the_Moscow_Kremlin.jpg",
                WorkingHours = "10:00–17:00, выходной — четверг",
                TicketPrice = 700
            },
            new Attraction
            {
                Id = 3,
                CityId = 1,
                Name = "Третьяковская галерея",
                Description = "Крупнейший в мире музей русского изобразительного искусства.",
                History = "Основана купцом Павлом Третьяковым в 1856 году. В 1892 году он передал собрание городу Москве. Сегодня галерея насчитывает более 190 000 экспонатов — иконы, живопись, скульптуру и графику от XI века до современности.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Tretyakov_gallery_main_facade.jpg/800px-Tretyakov_gallery_main_facade.jpg",
                WorkingHours = "10:00–18:00 (чт, пт — до 21:00), выходной — понедельник",
                TicketPrice = 500
            },
            new Attraction
            {
                Id = 4,
                CityId = 2,
                Name = "Эрмитаж",
                Description = "Один из крупнейших и старейших музеев мира, расположенный в Зимнем дворце.",
                History = "Основан Екатериной II в 1764 году. Комплекс состоит из 6 зданий, главное — Зимний дворец, бывшая резиденция российских императоров. Коллекция насчитывает более 3 миллионов экспонатов, в том числе произведения Рембрандта, Рубенса, Матисса и Пикассо.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/51/Hermitage_museum.jpg/800px-Hermitage_museum.jpg",
                WorkingHours = "10:30–18:00 (ср — до 21:00), выходной — понедельник",
                TicketPrice = 600
            },
            new Attraction
            {
                Id = 5,
                CityId = 2,
                Name = "Петергоф",
                Description = "Дворцово-парковый ансамбль с уникальной системой фонтанов, «Русский Версаль».",
                History = "Основан Петром I в 1714 году как летняя императорская резиденция. Знаменитая фонтанная система включает более 150 фонтанов и 4 каскада. В период Второй мировой войны дворцы были разрушены, но тщательно восстановлены в послевоенные годы.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4d/Peterhof_Grand_Palace.jpg/800px-Peterhof_Grand_Palace.jpg",
                WorkingHours = "10:00–18:00, выходной — понедельник (сезонно)",
                TicketPrice = 1000
            },
            new Attraction
            {
                Id = 6,
                CityId = 3,
                Name = "Казанский Кремль",
                Description = "Уникальный пример сочетания татарского и русского архитектурных стилей.",
                History = "Кремль возник на месте ханской цитадели. После взятия Казани Иваном Грозным в 1552 году был перестроен в камне русскими зодчими. Включает мечеть Кул-Шариф и православный Благовещенский собор. Объект Всемирного наследия ЮНЕСКО с 2000 года.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1e/Kazan_Kremlin_Annunciation_Cathedral.jpg/800px-Kazan_Kremlin_Annunciation_Cathedral.jpg",
                WorkingHours = "Территория — круглосуточно; музеи — 10:00–18:00",
                TicketPrice = 350
            },
            new Attraction
            {
                Id = 7,
                CityId = 4,
                Name = "Новосибирский оперный театр",
                Description = "Крупнейший в России оперный театр, архитектурная доминанта Новосибирска.",
                History = "Строительство театра началось в 1931 году и завершилось в 1945-м. Здание является крупнейшим в России театральным сооружением. Купол театра по диаметру превосходит московский Большой театр. Вмещает более 1700 зрителей и является гордостью всей Сибири.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Novosibirsk_Opera_Theatre.jpg/800px-Novosibirsk_Opera_Theatre.jpg",
                WorkingHours = "10:00–19:00, выходной — понедельник",
                TicketPrice = 600
            },
            new Attraction
            {
                Id = 8,
                CityId = 4,
                Name = "Академгородок",
                Description = "Уникальный научный центр в сосновом лесу — «наукоград» мирового значения.",
                History = "Основан в 1957 году по инициативе академика Лаврентьева. Здесь расположены десятки институтов Сибирского отделения РАН и Новосибирский государственный университет. В советское время был закрытым городом с особым статусом. Сегодня — один из ведущих научных центров России.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4a/Akademgorodok_1.jpg/800px-Akademgorodok_1.jpg",
                WorkingHours = "Открытая территория",
                TicketPrice = null
            },
            new Attraction
            {
                Id = 9,
                CityId = 5,
                Name = "Ельцин Центр",
                Description = "Современный культурный центр, посвящённый первому президенту России.",
                History = "Открыт в 2015 году. Включает музей 1990-х, выставочные залы, кинотеатр и общественные пространства. Экспозиция рассказывает о реформах и становлении новой России. Стал одним из самых современных культурных пространств страны.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6e/Yeltsin_Center_Ekaterinburg.jpg/800px-Yeltsin_Center_Ekaterinburg.jpg",
                WorkingHours = "10:00–21:00, без выходных",
                TicketPrice = 400
            },
            new Attraction
            {
                Id = 10,
                CityId = 5,
                Name = "Храм-на-Крови",
                Description = "Православный собор, построенный на месте расстрела семьи последнего российского императора.",
                History = "Возведён в 2000–2003 годах на месте дома Ипатьева, где в ночь с 16 на 17 июля 1918 года был расстрелян Николай II с семьёй. Храм является местом паломничества православных христиан со всего мира.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Yekaterinburg_ChurchOnBlood.jpg/800px-Yekaterinburg_ChurchOnBlood.jpg",
                WorkingHours = "7:00–23:00, без выходных",
                TicketPrice = null
            },
            new Attraction
            {
                Id = 11,
                CityId = 6,
                Name = "Нижегородский кремль",
                Description = "Средневековая крепость XVI века — символ и исторический центр Нижнего Новгорода.",
                History = "Строительство каменного кремля велось с 1500 по 1515 год. Крепость ни разу не была взята штурмом. На территории находятся Архангельский собор, Вечный огонь и административные здания. Расположен на Дятловых горах над слиянием Оки и Волги.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3b/Nizhny_novgorod_kremlin1.jpg/800px-Nizhny_novgorod_kremlin1.jpg",
                WorkingHours = "Территория — 8:00–20:00; музеи — 10:00–17:00",
                TicketPrice = 200
            },
            new Attraction
            {
                Id = 12,
                CityId = 7,
                Name = "Олимпийский парк",
                Description = "Комплекс спортивных арен, построенных к зимним Олимпийским играм 2014 года.",
                History = "Построен в Имеретинской низменности специально для Олимпиады-2014. Включает ледовые арены, стадион «Фишт» и другие объекты. После Олимпиады стал площадкой для крупных спортивных и музыкальных мероприятий.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Sochi_Olympic_Park.jpg/800px-Sochi_Olympic_Park.jpg",
                WorkingHours = "Круглосуточно (территория)",
                TicketPrice = null
            },
            new Attraction
            {
                Id = 13,
                CityId = 8,
                Name = "Золотой мост",
                Description = "Вантовый мост через бухту Золотой Рог — один из символов современного Владивостока.",
                History = "Открыт в 2012 году к саммиту АТЭС. Главный пролёт составляет 737 метров. Вместе с Русским мостом стал символом нового Владивостока и одной из крупнейших инфраструктурных строек России начала XXI века.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d6/Golden_Bridge_Vladivostok.jpg/800px-Golden_Bridge_Vladivostok.jpg",
                WorkingHours = "Круглосуточно",
                TicketPrice = null
            },
            new Attraction
            {
                Id = 14,
                CityId = 9,
                Name = "Новгородский детинец",
                Description = "Древний кремль IX–XV веков — объект Всемирного наследия ЮНЕСКО.",
                History = "Один из древнейших кремлей России. Нынешние каменные стены возведены в 1044–1430 годах. На территории находятся Софийский собор (1045–1050) — один из старейших храмов России — и Грановитая палата. Включён в список Всемирного наследия ЮНЕСКО в 1992 году.",
                PhotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b0/Novgorod-kremlin.jpg/800px-Novgorod-kremlin.jpg",
                WorkingHours = "Территория — 6:00–24:00; музеи — 10:00–18:00, выходной — вторник",
                TicketPrice = 250
            }
        );
    }
}
