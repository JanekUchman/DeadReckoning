The game is currently set to run when 4 players connect, 3 clients and 1 host.
The game is hosted by default on localhost, and port 6321. the IP can be changed but the port cannot.
Once a build is running on the "Host Game" part of the menu the game will autostart when 3 clients click "Join Game" then "Connect", optionally changing the IP.
Controls are: WASD to move, space to jump, left click to shoot

Bugs: Occasionally a packet for health/shots may be lost, I believe this is due to the way I'm serializing the data not the networking as it's on TCP