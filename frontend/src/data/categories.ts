export interface CategoryItem {
  slug: string
  label: string
}

export interface CategorySub {
  slug: string
  label: string
  items: CategoryItem[]
}

export interface CategoryMain {
  slug: string
  label: string
  emoji: string
  icon: string
  subs: CategorySub[]
}

export const CATEGORY_ICONS: Record<string, string> = {
  odiah:          'M15.75 10.5V6a3.75 3.75 0 10-7.5 0v4.5m11.356-1.993 1.263 12c.07.665-.45 1.243-1.119 1.243H4.25a1.125 1.125 0 01-1.12-1.243l1.264-12A1.125 1.125 0 015.513 7.5h12.974c.576 0 1.059.435 1.119 1.007z',
  elektronika:    'M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25A2.25 2.25 0 015.25 3h13.5A2.25 2.25 0 0121 5.25z',
  'dim-sad':      'M2.25 12l8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25',
  transport:      'M8.25 18.75a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m3 0h6m-9 0H3.375a1.125 1.125 0 01-1.125-1.125V14.25m17.25 4.5a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m3 0h1.125c.621 0 1.129-.504 1.09-1.124a17.902 17.902 0 00-3.213-9.193 2.056 2.056 0 00-1.58-.86H14.25M16.5 18.75h-2.25m0-11.177v-.958c0-.568-.422-1.048-.987-1.106a48.554 48.554 0 00-10.026 0 1.106 1.106 0 00-.987 1.106v7.635m12-6.677v6.677m0 4.5v-4.5m0 0h-12',
  'sport-hobby':  'M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z',
  'knyhy-media':  'M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0018 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25',
  'krasa-zdorovia': 'M9.813 15.904 9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715 18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 00-2.456 2.456z',
  'dytachi-tovary': 'M21 11.25v8.25a1.5 1.5 0 01-1.5 1.5H5.25a1.5 1.5 0 01-1.5-1.5v-8.25M12 4.875A2.625 2.625 0 1014.625 7.5H12m0-2.625V7.5m0-2.625A2.625 2.625 0 109.375 7.5H12m0 0H9.375M12 7.5v13.5m-3-13.5h-1.5a1.5 1.5 0 000 3H9m0-3v3m0 0h6m0-3h1.5a1.5 1.5 0 110 3H15m0-3v3',
  default:        'M4 6h16M4 12h16M4 18h16',
}

export const CATEGORY_TREE: CategoryMain[] = [
  {
    slug: 'odiah', label: 'Одяг і взуття', emoji: '👗',
    icon: CATEGORY_ICONS['odiah'],
    subs: [
      { slug: 'zhinochyi-odiah', label: 'Жіночий одяг', items: [
        { slug: 'platia', label: 'Плаття та сукні' },
        { slug: 'topy', label: 'Топи та футболки' },
        { slug: 'sorochky', label: 'Сорочки та блузи' },
        { slug: 'svetry', label: 'Светри та кардигани' },
        { slug: 'jeansy-zh', label: 'Джинси' },
        { slug: 'штany-zh', label: 'Штани та лосини' },
        { slug: 'спідниці', label: 'Спідниці' },
        { slug: 'кurtky-zh', label: 'Куртки та пальто' },
        { slug: 'sport-zh', label: 'Спортивний одяг' },
        { slug: 'bilyzen-zh', label: 'Нижня білизна' },
      ]},
      { slug: 'cholovichyi-odiah', label: 'Чоловічий одяг', items: [
        { slug: 'futbolky-ch', label: 'Футболки та поло' },
        { slug: 'sorochky-ch', label: 'Сорочки' },
        { slug: 'svetry-ch', label: 'Светри та толстовки' },
        { slug: 'jeansy-ch', label: 'Джинси' },
        { slug: 'штany-ch', label: 'Штани та шорти' },
        { slug: 'kurtky-ch', label: 'Куртки та пальто' },
        { slug: 'kostyumy', label: 'Костюми та піджаки' },
        { slug: 'sport-ch', label: 'Спортивний одяг' },
      ]},
      { slug: 'dytyachyi-odiah', label: 'Дитячий одяг', items: [
        { slug: 'dlya-divchatok', label: 'Для дівчаток' },
        { slug: 'dlya-hlopciv', label: 'Для хлопчиків' },
        { slug: 'dlya-nemovlat', label: 'Для немовлят 0–2 р.' },
        { slug: 'shkilna-forma', label: 'Шкільна форма' },
      ]},
      { slug: 'vzuttya-zhinoche', label: 'Взуття жіноче', items: [
        { slug: 'krosivky-zh', label: 'Кросівки та кеди' },
        { slug: 'tufli', label: 'Туфлі та балетки' },
        { slug: 'cherevyky-zh', label: 'Черевики' },
        { slug: 'sandali-zh', label: 'Сандалі та босоніжки' },
        { slug: 'choboty-zh', label: 'Чоботи' },
        { slug: 'kaблуки', label: 'На підборах' },
      ]},
      { slug: 'vzuttya-choloviche', label: 'Взуття чоловіче', items: [
        { slug: 'krosivky-ch', label: 'Кросівки' },
        { slug: 'tufli-ch', label: 'Туфлі та мокасини' },
        { slug: 'cherevyky-ch', label: 'Черевики' },
        { slug: 'kedy-ch', label: 'Кеди та сліпони' },
        { slug: 'choboty-ch', label: 'Чоботи' },
      ]},
      { slug: 'vzuttya-dytache', label: 'Взуття дитяче', items: [
        { slug: 'vzuttya-divch', label: 'Для дівчаток' },
        { slug: 'vzuttya-хlopch', label: 'Для хлопчиків' },
        { slug: 'vzuttya-babies', label: 'Для малюків' },
      ]},
      { slug: 'aksesuary', label: 'Аксесуари', items: [
        { slug: 'sumky', label: 'Сумки та рюкзаки' },
        { slug: 'gamanets', label: 'Гаманці' },
        { slug: 'prykorasy', label: 'Прикраси' },
        { slug: 'godynnyky', label: 'Годинники' },
        { slug: 'okuliary', label: 'Окуляри та оправи' },
        { slug: 'shapky', label: 'Шапки та кепки' },
        { slug: 'sharfy', label: 'Шарфи та рукавиці' },
        { slug: 'remeni', label: 'Ремені' },
      ]},
    ]
  },
  {
    slug: 'elektronika', label: 'Електроніка', emoji: '💻',
    icon: CATEGORY_ICONS['elektronika'],
    subs: [
      { slug: 'telefony', label: 'Телефони', items: [
        { slug: 'iphone', label: 'iPhone' },
        { slug: 'samsung', label: 'Samsung' },
        { slug: 'xiaomi', label: 'Xiaomi' },
        { slug: 'android-inshi', label: 'Інші Android' },
        { slug: 'telefon-aksesuары', label: 'Аксесуари для телефонів' },
      ]},
      { slug: 'noutbuky-pk', label: 'Ноутбуки та ПК', items: [
        { slug: 'noutbuky', label: 'Ноутбуки' },
        { slug: 'nastilni-pk', label: 'Настільні ПК' },
        { slug: 'monitory', label: 'Монітори' },
        { slug: 'komplektuiuchi', label: 'Комплектуючі' },
        { slug: 'peryferiya', label: 'Периферія' },
      ]},
      { slug: 'planshety', label: 'Планшети', items: [
        { slug: 'ipad', label: 'iPad' },
        { slug: 'android-planshety', label: 'Android планшети' },
        { slug: 'planshety-aksesuary', label: 'Аксесуари' },
      ]},
      { slug: 'tv-audio', label: 'ТВ та аудіо', items: [
        { slug: 'televizory', label: 'Телевізори' },
        { slug: 'navushnyky', label: 'Навушники' },
        { slug: 'kolonky', label: 'Колонки та саундбари' },
        { slug: 'audiosystemy', label: 'Аудіосистеми' },
      ]},
      { slug: 'foto-video', label: 'Фото та відео', items: [
        { slug: 'fotoaparaty', label: 'Фотоапарати' },
        { slug: 'obiektyny', label: "Об'єктиви" },
        { slug: 'videokamery', label: 'Відеокамери' },
        { slug: 'foto-aksesuary', label: 'Аксесуари' },
      ]},
      { slug: 'ihry-prystavi', label: 'Ігрові приставки', items: [
        { slug: 'playstation', label: 'PlayStation' },
        { slug: 'xbox', label: 'Xbox' },
        { slug: 'nintendo', label: 'Nintendo' },
        { slug: 'ihry', label: 'Ігри' },
        { slug: 'prystavi-aksesuary', label: 'Аксесуари' },
      ]},
      { slug: 'rozumni-prystroyi', label: 'Розумні пристрої', items: [
        { slug: 'smart-hodynnyky', label: 'Смарт-годинники' },
        { slug: 'tws-navushnyky', label: 'Навушники TWS' },
        { slug: 'smart-dim', label: 'Розумний дім' },
      ]},
    ]
  },
  {
    slug: 'dim-sad', label: 'Дім та сад', emoji: '🏠',
    icon: CATEGORY_ICONS['dim-sad'],
    subs: [
      { slug: 'mebli', label: 'Меблі', items: [
        { slug: 'dyvany', label: 'Дивани та крісла' },
        { slug: 'lizhka', label: 'Ліжка та матраси' },
        { slug: 'stoly-stiltsi', label: 'Столи та стільці' },
        { slug: 'shafy', label: 'Шафи та тумбочки' },
        { slug: 'dytachi-mebli', label: 'Дитячі меблі' },
        { slug: 'polytsi', label: 'Полиці та стелажі' },
      ]},
      { slug: 'pobutova-tekhnika', label: 'Побутова техніка', items: [
        { slug: 'kuhonna-tekhnika', label: 'Кухонна техніка' },
        { slug: 'pralni', label: 'Пральні машини' },
        { slug: 'holodylnyky', label: 'Холодильники' },
        { slug: 'pylososy', label: 'Пилососи' },
        { slug: 'driبna-tekhnika', label: 'Дрібна техніка' },
      ]},
      { slug: 'kuhnia', label: 'Кухня та столова', items: [
        { slug: 'posud', label: 'Посуд' },
        { slug: 'prybory', label: 'Столові прибори' },
        { slug: 'kuhonne-nachynnia', label: 'Кухонне начиння' },
      ]},
      { slug: 'tekstyl', label: 'Текстиль', items: [
        { slug: 'postilna-bilyzna', label: 'Постільна білизна' },
        { slug: 'rushnyky', label: 'Рушники' },
        { slug: 'fіranky', label: 'Штори та гардини' },
        { slug: 'kylýmy', label: 'Килими та пледи' },
      ]},
      { slug: 'dekor', label: 'Декор', items: [
        { slug: 'kartyny', label: 'Картини та постери' },
        { slug: 'vazy', label: 'Вази та фігурки' },
        { slug: 'dzerkala', label: 'Дзеркала' },
        { slug: 'svichky', label: 'Свічки та аромати' },
      ]},
      { slug: 'sad-horod', label: 'Сад і город', items: [
        { slug: 'sadovyi-inventar', label: 'Садовий інвентар' },
        { slug: 'sadovi-mebli', label: 'Садові меблі' },
        { slug: 'roslyny', label: 'Рослини та насіння' },
        { slug: 'barbeku', label: 'Барбекю та грилі' },
      ]},
    ]
  },
  {
    slug: 'transport', label: 'Транспорт', emoji: '🚗',
    icon: CATEGORY_ICONS['transport'],
    subs: [
      { slug: 'avtomobili', label: 'Автомобілі', items: [
        { slug: 'lehkovi-avto', label: 'Легкові автомобілі' },
        { slug: 'позашляховики', label: 'Позашляховики та SUV' },
        { slug: 'minivenы', label: 'Мінівени та мікроавтобуси' },
        { slug: 'elektrychni-avto', label: 'Електромобілі' },
      ]},
      { slug: 'motorezky', label: 'Мото', items: [
        { slug: 'motocykly', label: 'Мотоцикли' },
        { slug: 'skutery', label: 'Скутери та мопеди' },
        { slug: 'kvadrotsykly', label: 'Квадроцикли та ATV' },
      ]},
      { slug: 'zapchastyny', label: 'Запчастини', items: [
        { slug: 'kuzov', label: 'Кузов та зовнішні деталі' },
        { slug: 'dvyhun', label: 'Двигун та КПП' },
        { slug: 'khodova', label: 'Ходова та підвіска' },
        { slug: 'shyny-dysky', label: 'Шини та диски' },
        { slug: 'elektryka-avto', label: 'Електрика та оптика' },
      ]},
      { slug: 'velosypedy', label: 'Велосипеди', items: [
        { slug: 'hirski-velo', label: 'Гірські' },
        { slug: 'miske-velo', label: 'Міські та дорожні' },
        { slug: 'dytяchi-velo', label: 'Дитячі' },
        { slug: 'elektrovelo', label: 'Електровелосипеди та самокати' },
      ]},
      { slug: 'avtoaksesuary', label: 'Авто аксесуари', items: [
        { slug: 'gps', label: 'GPS та відеореєстратори' },
        { slug: 'avtokhimiya', label: 'Автохімія' },
        { slug: 'tюнинг', label: 'Тюнінг та стайлінг' },
      ]},
    ]
  },
  {
    slug: 'sport-hobby', label: 'Спорт та хобі', emoji: '⚽',
    icon: CATEGORY_ICONS['sport-hobby'],
    subs: [
      { slug: 'fitness', label: 'Фітнес та тренажери', items: [
        { slug: 'trenajery', label: 'Тренажери' },
        { slug: 'hantelі', label: 'Гантелі та штанги' },
        { slug: 'kilymky', label: 'Килимки та аксесуари' },
        { slug: 'велотренажери', label: 'Велотренажери та біговики' },
      ]},
      { slug: 'komandni-sporty', label: 'Командні види спорту', items: [
        { slug: 'futbol', label: 'Футбол' },
        { slug: 'basketbol', label: 'Баскетбол' },
        { slug: 'volejbol', label: 'Волейбол' },
        { slug: 'tenis', label: 'Теніс та бадмінтон' },
      ]},
      { slug: 'aktyvnyi-vidpochynok', label: 'Активний відпочинок', items: [
        { slug: 'turyzm-kempinh', label: 'Туризм і кемпінг' },
        { slug: 'rybolovlia', label: 'Риболовля' },
        { slug: 'polyvanni-а', label: 'Полювання' },
      ]},
      { slug: 'zimovi-sporty', label: 'Зимові види спорту', items: [
        { slug: 'lizhi', label: 'Лижі та палки' },
        { slug: 'snoubord', label: 'Сноубород' },
        { slug: 'kovzany', label: 'Ковзани' },
      ]},
      { slug: 'hobby', label: 'Хобі та дозвілля', items: [
        { slug: 'nastilni-ihry', label: 'Настільні ігри та пазли' },
        { slug: 'kolektsionuvannia', label: 'Колекціонування' },
        { slug: 'rukodillia', label: 'Рукоділля та в\'язання' },
        { slug: 'zhyvopys', label: 'Живопис і малювання' },
      ]},
    ]
  },
  {
    slug: 'knyhy-media', label: 'Книги та медіа', emoji: '📚',
    icon: CATEGORY_ICONS['knyhy-media'],
    subs: [
      { slug: 'knyhy', label: 'Книги', items: [
        { slug: 'khudozhnia', label: 'Художня literatura' },
        { slug: 'naukova', label: 'Наукова та навчальна' },
        { slug: 'dytiacha-lit', label: 'Дитяча literatura' },
        { slug: 'komiksy', label: 'Комікси та manga' },
      ]},
      { slug: 'muzychni-instrumenty', label: 'Музичні інструменти', items: [
        { slug: 'hitary', label: 'Гітари' },
        { slug: 'klavishni', label: 'Клавішні' },
        { slug: 'dudkovi', label: 'Духові' },
        { slug: 'ударні', label: 'Ударні' },
        { slug: 'aksesuary-muz', label: 'Аксесуари' },
      ]},
      { slug: 'ihry-іграшки', label: 'Ігри', items: [
        { slug: 'video-ihry', label: 'Відеоігри' },
        { slug: 'nastilni-ihry-knyhy', label: 'Настільні ігри' },
      ]},
    ]
  },
  {
    slug: 'krasa-zdorovia', label: 'Краса та здоров\'я', emoji: '💄',
    icon: CATEGORY_ICONS['krasa-zdorovia'],
    subs: [
      { slug: 'dohliad-shkira', label: 'Догляд за шкірою', items: [
        { slug: 'ochyshchennia', label: 'Очищення та тоніки' },
        { slug: 'зволоженnia', label: 'Зволоження та креми' },
        { slug: 'soniachnyy-zakhyst', label: 'Сонцезахисні засоби' },
        { slug: 'serovaтky', label: 'Сироватки та маски' },
      ]},
      { slug: 'makiyazh', label: 'Макіяж', items: [
        { slug: 'oblychchya-make', label: 'Основа та тональний крем' },
        { slug: 'ochi-make', label: 'Тіні та підводки' },
        { slug: 'huby-make', label: 'Помади та блиск' },
        { slug: 'nihti', label: 'Лаки для нігтів' },
      ]},
      { slug: 'parfumeria', label: 'Парфумерія', items: [
        { slug: 'zhinochi-aromaty', label: 'Жіночі аромати' },
        { slug: 'cholovichi-aromaty', label: 'Чоловічі аромати' },
        { slug: 'unisex-aromaty', label: 'Унісекс' },
      ]},
      { slug: 'volossia', label: 'Волосся', items: [
        { slug: 'shampuni', label: 'Шампуні та кондиціонери' },
        { slug: 'ukladka', label: 'Стайлінг та укладка' },
        { slug: 'farby-volos', label: 'Фарби для волосся' },
      ]},
    ]
  },
  {
    slug: 'dytяchi-tovary', label: 'Дитячі товари', emoji: '🧸',
    icon: CATEGORY_ICONS['dytachi-tovary'],
    subs: [
      { slug: 'ihrashky', label: 'Іграшки', items: [
        { slug: 'myaki-ihrashky', label: "М'які іграшки" },
        { slug: 'konstruktory', label: 'Конструктори та LEGO' },
        { slug: 'liялky', label: 'Ляльки та аксесуари' },
        { slug: 'mashynky', label: 'Машинки та техніка' },
        { slug: 'rozvyvalny', label: 'Розвивальні іграшки' },
      ]},
      { slug: 'koliаsky', label: 'Коляски та автокрісла', items: [
        { slug: 'koliаsky-pr', label: 'Коляски' },
        { slug: 'avtokrisla', label: 'Автокрісла' },
        { slug: 'nositski', label: 'Слінги та рюкзаки' },
      ]},
      { slug: 'shkilne', label: 'Школа', items: [
        { slug: 'rypzaky-sh', label: 'Рюкзаки шкільні' },
        { slug: 'kanchelyariya', label: 'Канцелярія' },
      ]},
    ]
  },
]


export const CONDITIONS = [
  { slug: 'new_with_tags',    label: 'Нове з ярликами',    badge: 'bg-teal-150 text-teal-700 border border-teal-300' },
  { slug: 'new_without_tags', label: 'Нове без ярликів',   badge: 'bg-teal-150 text-teal-700 border border-teal-300' },
  { slug: 'very_good',        label: 'Дуже гарний стан',   badge: 'bg-teal-100 text-teal-700 border border-teal-200' },
  { slug: 'good',             label: 'Гарний стан',         badge: 'bg-ivory-300 text-gray-600 border border-ivory-400' },
  { slug: 'satisfactory',     label: 'Задовільний стан',    badge: 'bg-gray-100 text-gray-500 border border-gray-200' },
] as const

export type ConditionSlug = (typeof CONDITIONS)[number]['slug']

export function getConditionLabel(slug?: string | null): string {
  return CONDITIONS.find(c => c.slug === slug)?.label ?? ''
}
export function getConditionBadge(slug?: string | null): string {
  return CONDITIONS.find(c => c.slug === slug)?.badge ?? 'bg-gray-100 text-gray-700'
}


export const COLORS = [
  { slug: 'black',     label: 'Чорний',    hex: '#1a1a1a' },
  { slug: 'white',     label: 'Білий',     hex: '#f5f5f5' },
  { slug: 'gray',      label: 'Сірий',     hex: '#9e9e9e' },
  { slug: 'beige',     label: 'Бежевий',   hex: '#d2b48c' },
  { slug: 'brown',     label: 'Коричневий',hex: '#8b4513' },
  { slug: 'red',       label: 'Червоний',  hex: '#e53935' },
  { slug: 'pink',      label: 'Рожевий',   hex: '#f48fb1' },
  { slug: 'orange',    label: 'Помаранчевий', hex: '#fb8c00' },
  { slug: 'yellow',    label: 'Жовтий',    hex: '#fdd835' },
  { slug: 'green',     label: 'Зелений',   hex: '#43a047' },
  { slug: 'teal',      label: 'Бірюзовий', hex: '#009688' },
  { slug: 'blue',      label: 'Синій',     hex: '#1e88e5' },
  { slug: 'navy',      label: 'Темно-синій', hex: '#1a237e' },
  { slug: 'purple',    label: 'Фіолетовий',hex: '#8e24aa' },
  { slug: 'gold',      label: 'Золотистий',hex: '#ffc107' },
  { slug: 'silver',    label: 'Сріблястий',hex: '#bdbdbd' },
  { slug: 'multicolor',label: 'Різнокольоровий', hex: 'linear-gradient(135deg,#f00,#0f0,#00f)' },
]

export function getColorLabel(slug?: string | null): string {
  return COLORS.find(c => c.slug === slug)?.label ?? ''
}


const CLOTHING_SIZES_ADULT = ['XXS','XS','S','M','L','XL','2XL','3XL','4XL+']
const CLOTHING_SIZES_KIDS  = ['68','74','80','86','92','98','104','110','116','122','128','134','140','146','152','158','164','170']
const SHOES_SIZES_WOMEN    = ['34','35','36','37','38','39','40','41','42']
const SHOES_SIZES_MEN      = ['38','39','40','41','42','43','44','45','46','47','48']
const SHOES_SIZES_KIDS     = ['16','18','20','22','24','26','28','30','32','34','36','38']

const SIZE_MAP: Record<string, string[]> = {
  'zhinochyi-odiah':      CLOTHING_SIZES_ADULT,
  'cholovichyi-odiah':    CLOTHING_SIZES_ADULT,
  'dytyachyi-odiah':      CLOTHING_SIZES_KIDS,
  'vzuttya-zhinoche':     SHOES_SIZES_WOMEN,
  'vzuttya-choloviche':   SHOES_SIZES_MEN,
  'vzuttya-dytache':      SHOES_SIZES_KIDS,
}

export function getSizesForSub(subSlug?: string | null): string[] {
  return subSlug ? (SIZE_MAP[subSlug] ?? []) : []
}

export function hasSizes(subSlug?: string | null): boolean {
  return !!(subSlug && SIZE_MAP[subSlug])
}

export const ALL_CATEGORIES_FLAT: { main: string; sub: string; item?: string; label: string }[] = []

for (const main of CATEGORY_TREE) {
  for (const sub of main.subs) {
    ALL_CATEGORIES_FLAT.push({ main: main.slug, sub: sub.slug, label: `${main.label} / ${sub.label}` })
    for (const item of sub.items) {
      ALL_CATEGORIES_FLAT.push({ main: main.slug, sub: sub.slug, item: item.slug, label: `${main.label} / ${sub.label} / ${item.label}` })
    }
  }
}

export function getCategoryLabel(main?: string, sub?: string, item?: string): string {
  if (!main) return ''
  const m = CATEGORY_TREE.find(c => c.slug === main)
  if (!m) return main
  if (!sub) return m.label
  const s = m.subs.find(c => c.slug === sub)
  if (!s) return `${m.label} / ${sub}`
  if (!item) return `${m.label} / ${s.label}`
  const i = s.items.find(c => c.slug === item)
  return `${m.label} / ${s.label} / ${(i?.label ?? item)}`
}
