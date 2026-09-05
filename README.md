# Snake

A desktop Snake game with two wall modes, three speeds, pause, and SQLite leaderboards.

You play on a **20×20** grid. There is always **one apple**. Eating it grows the snake and places the next apple on a random empty cell. Runs are saved when they end (death, win, or quit), not on every move.

## Features

- **Solid walls** — hitting the border ends the run
- **Wrap around** — the snake leaves one side and enters the opposite side
- **Easy / Medium / Hard** — only the snake speed changes (180 / 110 / 70 ms per cell)
- **Pause** with Space; the in-memory game state is kept, the timer is stopped
- **Username** at first launch (3–16 characters: letters, digits, space, underscore)
- **Leaderboards** split by mode and difficulty:
  - highest score
  - longest survival time
  - longest snake
  - fewest ticks among runs that reached score ≥ 10
  - personal bests for the current user

Score is apples eaten. **Steps** are ticks the snake actually moved, not keypresses.

## Requirements

You need the **.NET 8 SDK**. Check with:

```bash
dotnet --version
```

It should print a version starting with `8.`. If `dotnet` is missing, install the SDK from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0). NuGet packages restore automatically on first build.

This project uses:

| Piece | Why |
|---|---|
| C# / .NET 8 | language and runtime |
| Avalonia 11 | desktop GUI |
| Microsoft.Data.Sqlite | local leaderboard database |
| xUnit | unit tests for game rules and SQLite |

## How to run

From this folder:

```bash
cd snake-game
dotnet run
```

The first run restores packages, then opens the window. Enter a username, then **Play game** or **Leaderboard**.

Build without running:

```bash
dotnet build
```

The compiled app is `bin/Debug/net8.0/SnakeGame`.

## Controls

| Key | Action |
|---|---|
| Arrow keys or WASD | turn |
| Space | pause / resume |
| Menu button | leave the run (saved as quit) |

You cannot reverse 180° into your own body. Up to two upcoming turns are buffered so corner turns feel responsive.

## How to test

From this folder:

```bash
dotnet test
```

That builds the app and the test project, then runs **xUnit** tests covering:

- moving and growing
- ignoring a reverse into the body
- solid-wall death vs wrap-around
- self-collision
- pause blocking movement
- food never spawning on the snake
- username validation
- saving a run and querying a leaderboard

There are no UI tests. Game rules live in `Game/GameState.cs` with no Avalonia types, so they can be tested without opening a window.

## Database

SQLite file (created on first launch):

```text
bin/Debug/net8.0/data/snake.db
```

Last username is stored next to it in `settings.json`.

The schema is created automatically (`CREATE TABLE IF NOT EXISTS`). One row is inserted per finished run: username, mode, difficulty, score, steps, length, duration, result, timestamps. Individual moves are **not** logged.

The database is **user data**, not source. `*.db` and `settings.json` are gitignored. Do not commit them.

## Project layout

```text
snake-game/
  Game/           rules (grid, movement, pause) — no UI
  Data/           SQLite schema, save, leaderboard queries
  Views/          username, menu, setup, board, leaderboard
  SnakeGame.Tests xUnit tests
```

Everything runs on the **UI thread**. A `DispatcherTimer` ticks the snake; there is no extra thread for game logic.

## Git

This folder is its own git repository. Commit source only. After you play, `git status` should not show `snake.db`.
