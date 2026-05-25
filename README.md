# TrustMarket

> Маркетплейс для безпечної купівлі-продажу товарів між фізичними особами з вбудованою антифрод-системою, захищеним чатом та інтеграцією платіжної системи Monobank.

---

## Автор

- **ПІБ**: Дмитришин Анастасія
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

- Реєстрація та авторизація користувачів (JWT)
- Верифікація особи через сервіс Дія
- Публікація, редагування та видалення оголошень
- Пошук оголошень за ключовими словами (PostgreSQL Full-Text Search)
- Захищений чат між покупцем та продавцем (SignalR)
- Антифрод-аналіз повідомлень у реальному часі (блокування номерів карток, телефонів, зовнішніх посилань)
- Можливість погодити ціну через систему пропозицій (offers)
- Запис та перегляд оголошення (viewing request)
- Оплата через Monobank з підтримкою split-платежів
- Доставка через Нову Пошту (генерація ТТН)
- Сповіщення в Telegram про події (оплата, нове повідомлення тощо)
- Система відгуків після завершення угоди

---

## Опис основних класів / файлів

| Клас / Файл | Призначення |
|---|---|
| `src/Services/UserService` | Реєстрація, авторизація, верифікація через Дію |
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

### 3. Створення `.env` файлу

Створити файл `.env` у корені проєкту:

```
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_SECRET=your_jwt_secret_here
MONOBANK_TOKEN=your_monobank_token
TELEGRAM_BOT_TOKEN=your_telegram_bot_token
```

### 4. Запуск через Docker

```bash
docker-compose up --build
```

Сервіси після запуску:

- **API Gateway**: http://localhost:5000
- **Swagger (UserService)**: http://localhost:5001/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **PostgreSQL**: localhost:5432

### 5. Запуск фронтенду окремо (опційно)

```bash
cd frontend
npm install
npm run dev
```

Фронтенд: http://localhost:5173

---

## API приклади

### Авторизація

**POST /api/auth/register**

```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "Іван",
  "lastName": "Коваль"
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
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Оголошення

**GET /api/advertisements?search=велосипед**

Пошук оголошень за ключовим словом.

**POST /api/advertisements**

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

**POST /api/orders**

```json
{
  "advertisementId": "...",
  "hasDelivery": true
}
```

**Response** містить посилання на оплату Monobank (`paymentUrl`).

---

## Інструкція для користувача

1. **Головна сторінка** — перегляд оголошень, пошук за назвою
2. **Реєстрація / Вхід** — створення акаунту або авторизація
3. **Створення оголошення** — кнопка "Додати оголошення", заповнити форму з ціною та фото
4. **Купівля**:
   - Відкрити оголошення → натиснути "Купити"
   - Можна запропонувати свою ціну через кнопку "Зробити пропозицію"
   - Перейти до оплати через Monobank
5. **Чат** — після підтвердження угоди відкривається захищений чат з продавцем
6. **Відгук** — після завершення угоди з'являється форма для відгуку

---

## Проблеми і рішення

| Проблема | Рішення |
|---|---|
| Сервіс не запускається | Перевірити чи Docker Desktop запущений |
| Помилка підключення до БД | Перевірити змінні `POSTGRES_USER` / `POSTGRES_PASSWORD` у `.env` |
| Webhook від Monobank не приходить | Потрібен публічний URL (ngrok для локальної розробки) |
| Frontend не бачить API | Перевірити налаштування проксі у `vite.config.ts` |

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

*(додайте скриншоти у папку `/screenshots/` та вставте їх тут)*
