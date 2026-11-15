# Мониторинг DemoApi

Лабораторный стенд использует Prometheus, Grafana, cAdvisor и Dozzle из учебного комплекта. Prometheus опрашивает DemoApi по адресу `demo-api:8080/metrics` в общей сети Docker. Grafana загружает дашборд `demo-api` из JSON и подключается к Prometheus по внутренней сети.

## Запуск

Нужны Docker Desktop в режиме Linux-контейнеров и PowerShell 7. Из корня репозитория задайте локальный пароль PostgreSQL и запустите:

```powershell
$env:DB_PASSWORD = 'choose-a-local-password'
$env:API_IMAGE = 'ghcr.io/temirkhanerbolatovich-coder/demo-api'
$env:API_TAG = '1.0.0'
$env:API_PORT = '18097'
$env:DB_PORT = '25437'
$env:CHAOS_ENABLED = 'true'
docker network create daryn-lab07-scrape
docker compose -p daryn-lab07 -f compose.yaml -f compose.monitoring.yaml up -d --no-build
docker compose -f monitoring/compose.yaml up -d
```

Если сеть уже существует, пропустите команду её создания. Правила и дашборд ожидают имя контейнера `daryn-lab07-api-1`. Сохраните параметр `-p daryn-lab07`.

| Сервис | Локальный адрес |
| --- | --- |
| DemoApi readiness | http://127.0.0.1:18097/health/ready |
| Prometheus | http://127.0.0.1:19097/targets |
| Grafana | http://127.0.0.1:13007 |
| cAdvisor | http://127.0.0.1:18087 |
| Dozzle | http://127.0.0.1:18887 |

На новом учебном экземпляре Grafana используйте `admin` и значение `GRAFANA_ADMIN_PASSWORD`. При отсутствии переменной начальный пароль — `admin`.

## Нагрузка и проверка

```powershell
docker compose -f monitoring/compose.yaml exec -T prometheus promtool check config /etc/prometheus/prometheus.yml
pwsh -File ./scripts/load.ps1 -BaseUrl http://127.0.0.1:18097 -DelayMs 650
```

Prometheus должен показать три цели UP. В дашборде Grafana **DemoApi** панель **Tasks created per minute** использует счётчик ответов 201 метода `Tasks.Create` и окно `[1m]`. Остановите генератор нагрузки клавишами Ctrl+C.

Включайте `CHAOS_ENABLED` только для учебных проверок. cAdvisor требует расширенных прав для чтения метрик хоста, Dozzle использует локальный Docker socket для просмотра журналов. Интерфейсы доступны через loopback.

## Остановка

```powershell
docker compose -f monitoring/compose.yaml stop
docker compose -p daryn-lab07 -f compose.yaml -f compose.monitoring.yaml stop
```
