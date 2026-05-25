# TrustMarket

> Маркетплейс для безпечної купівлі-продажу товарів між фізичними особами з вбудованою антифрод-системою, захищеним чатом та інтеграцією платіжної системи Monobank.

---

## Автор

- **ПІБ**: Дмитришин Анастасія Романівна
- **Група**: ФЕП-42
- **Керівник**: доц. Шувар Р. Я.
- **Дата виконання**: 25.05.2026

---

## Загальна інформація

- **Тип проєкту**: Вебзастосунок (мікросервісна архітектура)
- **Мова програмування**: C# (.NET 8), TypeScript (Vue 3)
- **Фреймворки / Бібліотеки**: ASP.NET Core, Entity Framework Core, MassTransit, SignalR, YARP, Vue 3, Pinia, Axios

---

## Опис функціоналу

- Реєстрація та авторизація користувачів (JWT + Google OAuth)
- Верифікація особи через сервіс Дія
- Публікація, редагування та видалення оголошень
- Пошук та фільтрація оголошень за ключовими словами, категорією та іншими параметрами 
- Перегляд оголошень на карті
- Захищений чат між покупцем та продавцем (SignalR)
- Антифрод-аналіз повідомлень у реальному часі (блокування номерів карток, телефонів, зовнішніх посилань)
- Можливість погодити ціну через систему пропозицій
- Запис на перегляд оголошення (viewing request) з автоматичним сповіщенням довіреної особи через email (SMTP) та Telegram
- Оплата через Monobank з підтримкою split-платежів
- Доставка через Нову Пошту
- Сповіщення в Telegram про події (оплата, нове повідомлення тощо)
- Система відгуків після завершення угоди

---

## Опис основних класів / файлів

| Клас / Файл | Призначення |
|---|---|
| `src/Services/UserService` | Реєстрація, авторизація, Google OAuth, верифікація через Дію |
| `src/Services/CatalogService` | Управління оголошеннями та пропозиціями |
| `src/Services/ChatService` | Чат, перегляди, антифрод-аналізатор |
| `src/Services/FinanceService` | Замовлення, оплата Monobank, доставка |
| `src/Services/NotificationService` | Telegram-сповіщення про події |
| `src/Services/ReviewService` | Відгуки після завершення угоди |
| `src/ApiGateway` | YARP reverse proxy, маршрутизація запитів |
| `frontend/src/views` | Сторінки Vue (головна, чат, оголошення, профіль) |
| `frontend/src/api` | Axios-клієнти до кожного сервісу |
| `RuleBasedFraudAnalyzer.cs` | Правила антифрод-аналізу повідомлень |
| `ProcessMonobankWebhookCommandHandler.cs` | Обробка webhook-подій від Monobank |
| `docker-compose.yml` | Опис усіх сервісів, баз даних та черги |
| `scripts/init-multiple-dbs.sh` | Ініціалізація окремих БД для кожного сервісу |

---

## Як запустити проєкт "з нуля"

### 1. Встановлення інструментів

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js v20+](https://nodejs.org/)

### 2. Клонування репозиторію

```bash
git clone https://github.com/nekleevaN/Diploma.git
cd Diploma
```

### 3. Файл `.env`

Файл `.env` вже присутній у репозиторії та містить:

```
TELEGRAM_BOT_TOKEN=...
TELEGRAM_ADMIN_CHAT_ID=...
```

Якщо потрібно тестувати Monobank webhook локально (через ngrok або аналог), додайте до `.env`:

```
MONOBANK_WEBHOOK_URL=https://your-ngrok-url.ngrok.io
```

Решта змінних (JWT, PostgreSQL, Monobank токен) задані безпосередньо у `docker-compose.yml` для зручності локальної розробки.

### 4. Запуск через Docker

```bash
docker-compose up --build
```

Сервіси після запуску:

| Сервіс | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API Gateway | http://localhost:5000 |
| UserService (Swagger) | http://localhost:5001/swagger |
| CatalogService | http://localhost:5002 |
| ChatService | http://localhost:5003 |
| NotificationService | http://localhost:5004 |
| FinanceService | http://localhost:5005 |
| ReviewService | http://localhost:5006 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| PostgreSQL | localhost:5432 |

### 5. Запуск фронтенду окремо (для розробки)

```bash
cd frontend
npm install
npm run dev
```

Фронтенд: http://localhost:3000

---

## Тестування

Проєкт містить unit та інтеграційні тести для кожного сервісу:

```
tests/
├── TrustMarket.UserService.UnitTests
├── TrustMarket.UserService.IntegrationTests
├── TrustMarket.CatalogService.UnitTests
├── TrustMarket.CatalogService.IntegrationTests
├── TrustMarket.ChatService.UnitTests
├── TrustMarket.ChatService.IntegrationTests
├── TrustMarket.FinanceService.UnitTests
├── TrustMarket.FinanceService.IntegrationTests
├── TrustMarket.ReviewService.UnitTests
├── TrustMarket.ReviewService.IntegrationTests
├── TrustMarket.NotificationService.IntegrationTests
└── TrustMarket.TestInfrastructure
```

Запуск усіх тестів:

```bash
dotnet test
```

---

## API приклади

### Авторизація

**POST /api/auth/register**

```json
{
  "firstName": "Іван",
  "lastName": "Коваль",
  "email": "user@example.com",
  "password": "Password123!",
  "passwordConfirm": "Password123!",
  "agreeToTerms": true
}
```

**POST /api/auth/login**

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Оголошення

**GET /api/ads?search=велосипед**

Пошук оголошень за ключовим словом. Підтримує також фільтри: `category`, `priceMin`, `priceMax`, `condition`, `brand`, `sortBy`, `page`, `pageSize`.

**POST /api/ads**

```json
{
  "title": "Велосипед Trek",
  "description": "Стан хороший, є фото",
  "price": 3500,
  "categoryId": "..."
}
```

---

### Замовлення та оплата

**POST /api/payment/create**

```json
{
  "advertisementId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sellerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "adTitle": "Велосипед Trek",
  "amount": 3500,
  "hasDelivery": true
}
```

**Response** містить посилання на оплату Monobank (`pageUrl`).

---

## Інструкція для користувача

1. **Головна сторінка** — перегляд оголошень, пошук за назвою
2. **Реєстрація / Вхід** — створення акаунту, авторизація або вхід через Google
3. **Створення оголошення** — кнопка "Додати оголошення", заповнити форму з ціною та фото
4. **Купівля**:
   - Відкрити оголошення → натиснути "Купити"
   - Можна запропонувати свою ціну через кнопку "Зробити пропозицію"
   - Ввести дані для доставки (якщо обрана доставка Новою Поштою)
   - Перейти до оплати через Monobank
5. **Перегляд** — можна домовитись про особистий перегляд товару; при підтвердженні перегляду довірена особа отримає сповіщення через email та Telegram
6. **Чат** — доступний одразу після відкриття оголошення, не потребує підтвердження угоди
7. **Відгук** — після завершення угоди з'являється форма для відгуку

---

## Проблеми і рішення

| Проблема | Рішення |
|---|---|
| Сервіс не запускається | Перевірити чи Docker Desktop запущений |
| Помилка підключення до БД | Переконатись, що контейнер `postgres` запустився (healthcheck) |
| Webhook від Monobank не приходить | Додати публічний URL у `.env` як `MONOBANK_WEBHOOK_URL` (ngrok для локальної розробки) |
| Frontend не бачить API | Перевірити налаштування проксі у `vite.config.ts` (dev) або `nginx.conf` (Docker) |
| Бази даних не створились | Перевірити, чи виконався скрипт `scripts/init-multiple-dbs.sh` |

---

## Використані джерела

- Документація ASP.NET Core: https://learn.microsoft.com/aspnet/core
- MassTransit документація: https://masstransit.io/documentation
- Vue 3 документація: https://vuejs.org/guide
- YARP документація: https://microsoft.github.io/reverse-proxy
- Monobank API: https://api.monobank.ua/docs
- PostgreSQL Full-Text Search: https://www.postgresql.org/docs/current/textsearch.html

---

## Screenshots

### Головна сторінка
![Головна сторінка](screenshots/home.png)

### Сторінка оголошення
![Оголошення](screenshots/ad-detail.png)

### Чат
![Чат](screenshots/chat.png)

### Checkout та оплата
![Оплата](screenshots/checkout.png)

### Карта оголошень
![Карта](screenshots/map.png)

### Профіль користувача
![Профіль](screenshots/profile.png)
