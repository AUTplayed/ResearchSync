# ResearchSync

This mod synchronizes journey mode research progress live across players on a server.

Assuming player A sacrifices 5 Wood for research, player B will also immediately receive these 5 Wood for their research progress.
Concurrent researching is properly handled.

On server join each client will sync their whole current state with all other clients, but only overriding the local research state if it's < than the one sent by another player.
Essentially this means that once all players have joined and synced on the server, each player will have a merged state of the highest research progress of each item from all players.
From that point on all sacrificed items are constantly synced across all players.

**Warning**: This behavior means that if you join with a character that has unlocked every item on a server, all other players will immediately also permanently unlock everything. 

## How it works

### Diff

Upon joining a server, we copy the research progress to a local cache.
In SendClientChanges we frequently check for differences between the cache and the actual player state. If a difference is detected, only the difference is sent to the other players.
Since we only ever send diffs, and the access to the local cache is handled via a thread lock, concurrent updates don't matter.

### Sync

Upon joining a server, we send out our full research progress to all players.
They can then update their local progress to the higher one of the two for each item, and send a Sync-Response back to the freshly joined player.
The new player then does the same with all the responses, but without sending another response (to not loop forever).
I wasn't able to get the playerId from the Connect event, so the Sync-Response is basically the workaround for that.