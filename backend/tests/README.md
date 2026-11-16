# Проверки главы 10

Тесты используют отдельную PostgreSQL-базу. Не указывайте рабочую базу:
интеграционные проверки применяют миграции и добавляют тестовых пользователей и клубы.
Каждый прогон создаёт новые данные и не удаляет существующие.

PowerShell, из корня репозитория:

```powershell
docker run --name pclub-ch10-tests --rm -d -e POSTGRES_DB=pclub_tests -e POSTGRES_USER=pclub -e POSTGRES_PASSWORD=chapter10-test-only -p 127.0.0.1:55433:5432 postgres:17-alpine
# Дождаться готовности PostgreSQL:
docker exec pclub-ch10-tests pg_isready -U pclub -d pclub_tests
$env:PCLUB_TEST_POSTGRES = 'Host=localhost;Port=55433;Database=pclub_tests;Username=pclub;Password=chapter10-test-only'
dotnet test PClub.slnx
docker stop pclub-ch10-tests
```

Только модульные проверки, без PostgreSQL:

```powershell
dotnet test PClub.slnx --filter 'FullyQualifiedName!~PostgreSql'
```

Проверяются точность цены, лимиты пополнения, нехватка средств, переполнение,
знаки записей журнала, возврат 100%/50%, округление нечётной копейки,
границы 24 и 2 часов, запрет поздней и повторной отмены.
Интеграционный сценарий проверяет миграции, отсутствие брони при нехватке средств,
откат конфликтующей брони, одновременную отмену, десять параллельных пополнений,
сверку баланса и отсутствие возвратов по старым бесплатным броням.
Сценарий запускается с повторами Npgsql и без них.

## Глава 11

Тот же `PCLUB_TEST_POSTGRES` используется тестами аутентификации. Классы PostgreSQL
выполняются последовательно, а конкурентные запросы создаются внутри сценариев.

Для HTTP-проверок нужен отдельный контейнер и база. Ключ ниже — только тестовый,
его нельзя использовать в рабочей конфигурации.

```powershell
docker run --name pclub-ch11-tests --rm -d -e POSTGRES_DB=pclub_http_tests -e POSTGRES_USER=pclub -e POSTGRES_PASSWORD=chapter11-test-only -p 127.0.0.1:55433:5432 postgres:17-alpine
docker exec pclub-ch11-tests pg_isready -U pclub -d pclub_http_tests
$env:ConnectionStrings__Postgres = 'Host=localhost;Port=55433;Database=pclub_http_tests;Username=pclub;Password=chapter11-test-only'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Jwt__SigningKey = 'test-only-key-not-used-outside-automated-tests-1234567890'
dotnet run --project backend/src/PClub.Api --no-launch-profile --urls http://127.0.0.1:55150
```

В другом терминале из корня проекта:

```powershell
$env:PCLUB_TEST_JWT = 'test-only-key-not-used-outside-automated-tests-1234567890'
python backend/tests/auth_http_checks.py
```

Скрипт создаёт данные исключительно в `pclub_http_tests`: пользователей, роли, клуб и место.
После проверки остановите API и выполните `docker stop pclub-ch11-tests`.
