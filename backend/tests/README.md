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
dotnet test PClub.slnx --filter 'FullyQualifiedName!~PostgreSqlMoneyFlowAndConcurrency'
```

Проверяются точность цены, лимиты пополнения, нехватка средств, переполнение,
знаки записей журнала, возврат 100%/50%, округление нечётной копейки,
границы 24 и 2 часов, запрет поздней и повторной отмены.
Интеграционный сценарий проверяет миграции, отсутствие брони при нехватке средств,
откат конфликтующей брони, одновременную отмену, десять параллельных пополнений,
сверку баланса и отсутствие возвратов по старым бесплатным броням.
Сценарий запускается с повторами Npgsql и без них.
