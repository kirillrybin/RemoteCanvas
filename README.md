[English](README.md) | [Русский](README.ru.md)

# RemoteCanvas

Unity demo for [com.kiriyo.sdui](https://github.com/kirillrybin/com.kiriyo.sdui) — a Server-Driven UI framework where the server controls layout, content, and navigation at runtime.

## Structure

```
RemoteCanvas.Unity/   — Unity project (iOS / Android, URP)
RemoteCanvas.Server/  — Node.js mock backend (json-server)
```

## Server

Node.js 18+, port 3000 by default.

```bash
cd RemoteCanvas.Server
npm install
npm run dev          # nodemon, auto-restart
npm run dev:8080     # alternate port
```

Endpoints:

| Method | Path | Description |
|---|---|---|
| GET | `/pages/:name` | Page JSON with A/B resolution and localization |
| GET | `/active_banner` | Currently active banner |
| PATCH | `/banners/:id/activate` | Switch active banner |
| GET | `/gallery_items` | Gallery items |
| POST | `/gallery_items/:id/like` | Like item |
| DELETE | `/gallery_items/:id/like` | Unlike item |
| GET | `/news` | News feed, sorted pinned-first |
| POST | `/reset` | Restore db to seed state |

Query params: `?userId=` for A/B group resolution, `?lang=` for localization (`en`, `ru`).

## Unity

1. Open `RemoteCanvas.Unity` in Unity 2022.3+
2. Set `BaseUrl` in `SDUIConfig` asset (e.g. `http://localhost:3000`)
3. Play — starts on `main_menu`

Dependencies resolved via UPM:

```json
"com.kiriyo.sdui":       "https://github.com/kirillrybin/com.kiriyo.sdui.git#v0.1.0",
"com.cysharp.unitask":   "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
```

## Architecture

- **`SDUIService`** — fetches page JSON, TTL cache keyed by `pageName|lang`
- **`UIBuilder`** — recursive JSON tree → uGUI hierarchy
- **`ComponentFactory`** — routes `type` string to `IComponentBuilder`
- **`ActionDispatcher`** — decoupled string-keyed action bus (`"key:payload"`)
- **`SDUINavigator`** — back-stack page navigation
- **`SDUIHttpClient`** — Unity WebRequest wrapper, injects `?lang=` on GET

Component builders: `button`, `text`, `image`, `panel`, `spacer`, `banner`, `gallery`, `news_feed`.

Debug panel (Editor / Development builds only): switch A/B group, language, active banner, navigate to any page.