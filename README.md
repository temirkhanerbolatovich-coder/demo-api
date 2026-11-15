# DemoApi

Учебный ASP.NET Core API для проектов и задач. Маршруты: /api/projects, /api/projects/{id}/stats и /api/tasks. Проверки доступности: /health/live и /health/ready.

## Docker Compose

Требуются Docker Desktop и Compose. Скопируйте .env.example в .env и задайте DB_PASSWORD. Локальный API доступен по http://127.0.0.1:18093, PostgreSQL по 127.0.0.1:25433.

    docker compose up -d --build --wait
    docker compose ps

## Разработка

Для режима InMemory запустите профиль http из DemoApi.slnx. Для PostgreSQL запустите docker compose up -d --wait db и задайте Storage=Postgres и ConnectionStrings__Db в окружении IDE. Используйте порт DB_PORT из .env.

## Тесты

В корне проекта выполните dotnet test DemoApi.slnx. Для интеграционных тестов PostgreSQL передайте те же параметры окружения и используйте отдельную учебную БД.

## Мониторинг

[Запуск локального стенда Prometheus и Grafana](monitoring/README.md).
