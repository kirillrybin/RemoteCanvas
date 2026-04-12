[English](README.md) | [Русский](README.ru.md)

# RemoteCanvas

Unity-демо для [com.kiriyo.sdui](https://github.com/kirillrybin/com.kiriyo.sdui) — фреймворка Server-Driven UI, где сервер управляет интерфейсом, контентом и навигацией в рантайме без обновления клиента.

## Структура

```
RemoteCanvas.Unity/   — Unity-проект (iOS / Android, URP)
RemoteCanvas.Server/  — Mock-бэкенд на Node.js (json-server)
```

## Сервер

Node.js 18+, порт 3000 по умолчанию.

```bash
cd RemoteCanvas.Server
npm install
npm run dev          # nodemon, авторестарт при изменениях
npm run dev:8080     # альтернативный порт
```

Эндпоинты:

| Метод | Путь | Описание |
|---|---|---|
| GET | `/pages/:name` | JSON страницы с A/B-выбором варианта и локализацией |
| GET | `/active_banner` | Текущий активный баннер |
| PATCH | `/banners/:id/activate` | Переключить активный баннер |
| GET | `/gallery_items` | Элементы галереи |
| POST | `/gallery_items/:id/like` | Лайк |
| DELETE | `/gallery_items/:id/like` | Убрать лайк |
| GET | `/news` | Лента новостей, закреплённые первыми |
| POST | `/reset` | Сбросить БД к исходным данным |

Query-параметры: `?userId=` для выбора A/B-варианта, `?lang=` для локализации (`en`, `ru`).

## Unity

1. Открыть `RemoteCanvas.Unity` в Unity 2022.3+
2. Указать `BaseUrl` в ассете `SDUIConfig` (например `http://localhost:3000`)
3. Запустить Play — откроется страница `main_menu`

Подключаемые зависимости через UPM:

```json
"com.kiriyo.sdui":       "https://github.com/kirillrybin/com.kiriyo.sdui.git#v0.1.1",
"com.cysharp.unitask":   "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
```

## Архитектура

- **`SDUIService`** — загрузка JSON страниц, TTL-кэш с ключом `pageName|lang`
- **`UIBuilder`** — рекурсивное построение uGUI-иерархии из JSON-дерева
- **`ComponentFactory`** — маршрутизация поля `type` к нужному `IComponentBuilder`
- **`ActionDispatcher`** — шина действий по строковому ключу (`"key:payload"`)
- **`SDUINavigator`** — навигация с историей (back-stack)
- **`SDUIHttpClient`** — обёртка над Unity WebRequest, автоматически добавляет `?lang=` к GET-запросам

Билдеры компонентов: `button`, `text`, `image`, `panel`, `spacer`, `banner`, `gallery`, `news_feed`.

Debug-панель (только в Editor / Development-сборках): переключение A/B-групп, языков и активного баннера, быстрый переход между страницами.