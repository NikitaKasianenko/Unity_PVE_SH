# Week 08 — Session-Based Host/Join Over the Internet

Host and join Deathmatch matches over the public internet using **Unity Gaming Services
(UGS)** — Authentication + Lobby + Relay + Netcode for GameObjects. No port forwarding, no
IP typing: the host publishes a lobby carrying a Relay join code, clients browse the lobby
list and connect through Relay (NAT traversal handled by UGS).

## Backend & why

- **Backend: Unity Gaming Services (Relay + Lobby + Authentication).** It's the native
  online-subsystem for a Unity/NGO project — first-party, free tier is enough (50 CCU Relay,
  25 lobbies), and it integrates directly with `UnityTransport`. Steam/EOS are Unreal-track
  options and would add an SDK the project doesn't need.
- **Authentication: anonymous sign-in** (`SignInAnonymouslyAsync`). Enough for class testing;
  for production you'd swap in Unity Player Accounts / Steam / another identity provider.
- **Relay connection type: `dtls`** (encrypted UDP).

## How it works

`Assets/Scripts/Multiplayer/Online/RelayLobbyManager.cs`
- `InitAndSignInAsync` — one-time `UnityServices.InitializeAsync` (gated on `UnityServices.State`)
  + anonymous sign-in. Sets a per-name **profile** so two Editors don't share one auth cache.
- `HostAsync` — (a) `CreateAllocationAsync(maxConnections)` → (b) `GetJoinCodeAsync` →
  (c) `SetRelayServerData(new RelayServerData(allocation,"dtls"))` → (d) `StartHost()`, then
  creates a **public lobby** with the join code stored in `options.Data["RelayJoinCode"]`, and
  runs a 15 s heartbeat so the lobby stays alive.
- `QueryLobbiesAsync` — `QueryLobbiesAsync()` → list of open lobbies for the browser.
- `JoinByLobbyIdAsync` — join lobby → read `RelayJoinCode` → `JoinAllocationAsync` →
  `SetRelayServerData(...)` → `StartClient()`.
- Deletes the lobby on quit.

`Assets/Scripts/Multiplayer/UI/GameMenuUI.cs` — online menu: name field, **HOST GAME**,
**REFRESH**, a live lobby browser (click a row to join), and a status line showing the join
code / errors. Team select and the rest of the Deathmatch flow are unchanged.

## One-time setup (you must do this — account/dashboard)

1. **Create a UGS project:** dashboard.unity.com → New project → note the **Project ID** (free tier).
2. **Link the Editor to it:** Unity → Edit ▸ Project Settings ▸ **Services**, sign in, select the project.
3. **Enable services** on the dashboard: **Authentication**, **Lobby**, **Relay**.
4. **Install packages** (Window ▸ Package Manager ▸ **+** ▸ *Add package by name*):
   - `com.unity.services.authentication`
   - `com.unity.services.relay`
   - `com.unity.services.lobby`
   (Package Manager auto-resolves versions compatible with this Unity version. The scripts
   won't compile until these are installed.)

> Relay API note: this project's Relay package (`com.unity.services.relay@6.x` on
> `com.unity.transport`) exposes neither the old `new RelayServerData(allocation, "dtls")`
> constructor nor the `ToRelayServerData` helper. `RelayLobbyManager` therefore builds the
> `RelayServerData` directly from the allocation's secure (dtls) endpoint — see `SelectEndpoint`.

## Testing the flow

1. Enter Play (Host machine) → type a name → **HOST GAME**. Status shows the Relay join code
   and you enter team select.
2. On a **second machine on a different network** (built standalone — see below), launch →
   **REFRESH** → your lobby appears → click it → it joins through Relay and drops into team select.
3. Both play together via Relay.

**Build a standalone client:** File ▸ Build Settings ▸ add the Deathmatch scene ▸ Build (Windows).
A standalone has its own auth cache, so it won't clash with the Editor. For two Editors on one
machine use **ParrelSync** (separate persistent data path per clone) — see the pitfall below.

## Known limitations

- Anonymous auth only — no persistent identity / friends / invites.
- Flat public lobby list (no filters, regions, or passwords).
- Free-tier caps: 50 CCU Relay, 25 lobbies.
- No reconnect/host-migration; if the host leaves, the match ends.
- Two Editors on the **same** machine share the anonymous auth cache unless you use ParrelSync
  or a standalone build (fixed here by setting a per-name profile, but a standalone is cleanest).

## Security / submission hygiene

- No credentials are committed. UGS anonymous auth stores no secret files in the repo; the
  Project ID (in `ProjectSettings`) is not a secret. `.gitignore` also blocks service-account
  keys, Steam/EOS secrets and `.env` files.
- **Add the demo recording here** as `Week08/demo.mp4` (kept out of git if large — record the
  full host → browse → join → play-together flow across the two networks).

## Deliverables in this folder

| File | What |
|---|---|
| `README.md` | This file (backend, auth, limitations, setup + test runbook). |
| `demo.mp4` | Screen recording of the end-to-end flow (add after testing). |
