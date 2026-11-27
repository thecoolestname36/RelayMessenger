# Shared Area - Agent Context

## Purpose
Shared DTOs and state: `NodeStateModel`, `NodeStateStore`, `SendMessageRequest`, `ClientMessageRequest`, `Routing`, crypto helpers.

## Business Logic Notes
- String-typed message DTO contract (Phase 1).
- State consistency: update store before broadcast.

## Phase Context
- Contract phase owner for DTO changes.
