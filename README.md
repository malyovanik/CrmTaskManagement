# Як запустити проект локально

## Для роботи з базою даних PostgresSQL я використовував докер,тому опишу як запустити з докером. Якщо у вас вже є своя база даних, то можна підключитись до неї, up for you.
Треба встановити:
1. .NET 8 SDK
2. Docker Desktop
3. Інструмент dotnet-ef — встановлюється командою:
   dotnet tool install --global dotnet-ef

## Крок 1. Клонувати репозиторій
git clone https://github.com/malyovanik/CrmTaskManagement.git
cd CrmTaskManagement

## Крок 2. Запустити базу даних PostgreSQL через Docker
docker run --name crm-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=crm_task_management -p 5432:5432 -d postgres:16

Перевірити, що контейнер запустився:
docker ps

Має з'явитись рядок з назвою crm-postgres і статусом Up.

## Крок 3. Зібрати проект
dotnet build

## Крок 4. Застосувати міграції (створити таблиці в базі)
dotnet ef database update --project CrmTaskManagement.Data --startup-project CrmTaskManagement.Console

## Крок 5. Запустити консольний застосунок
dotnet run --project CrmTaskManagement.Console

Це наповнить базу тестовими даними і запустить демонстрацію 
роботи бізнес-логіки: створення задачі, зміну статусу, отримання списку задач по 
виконавцю, а також покаже, що бізнес-правила спрацьовують як було описано.

У коноль буде виведено результат демо. WorkTaskDemoRunner

## Крок 6. Для зручності перевірки ще додав інтегрейшн тести з юніт тестами, які перевіряють роботу бізнес-логіки.
dotnet test

Тести самі піднімуть і закриють окремий тестовий контейнер PostgreSQL — вручну нічого 
додатково готувати не треба, головне щоб Docker Desktop був запущений.
