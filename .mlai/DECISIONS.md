# Decisions

## 2026-11-25: The game is a multiplayer tycoon, not a city builder
- Stated by the user: the player is the owner and manager of businesses (warung, toko, kafe) and rentals (kost, apartemen, kontrakan). NPC citizens are the ones who buy food and rent rooms. The player should not have to build houses for citizens.
- One map is shared by many players at the same time (not single player). This is the difference from an ordinary tycoon game.
- Status: direction confirmed, not implemented yet. Do not start the rework before the user says go.
- Gap in the code at the time of the decision:
  - Population is the sum of housing capacity the player built (`EconomyCalculator.HitungPopulasi`); citizens are not tenants.
  - Income is a fixed amount per building per day, on the client (`EconomyManager`) and on the server (`server/db.py` `_pemasukan_offline_terkunci`); no purchase or rent per citizen.
  - Walking citizens are decoration: they pick random road tiles and never enter a building (`PejalanKaki.cs`).
  - The server already has accounts, land ownership per tile, permits from the NPC mayor, city treasury, chat and leaderboard, but no citizens.
- Consequence: citizens (who they are, where they rent, where they shop) have to live on the server so every player sees the same customers and competes for them.
- `perancangan-game.md` still describes "players freely build anything" and "Single player"; it needs updating when the rework starts.
- Open: whether public facilities (school, clinic, park, town hall) belong to the city or are built by players.
