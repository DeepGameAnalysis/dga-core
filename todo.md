# TO-DOs:

# Supported games
- Check:

  - Starcraft 2
  - Company of Heroes 1/2
  - Age of Empires 2 HD (with latest DLC)
  - Teamfortress 2

# Functionality
- Export a file to replay the results of the encounter detection
- Export results as a movie file (.mp4, .avi etc)

# Code changes
- Adjust CSV exporter to prevent adding column names for each entry
- Implement spatial and temporal prunning
- Build tables for links and distances
- Add suitable data structures (trees etc)
- Split methods to detect encounters (eventbased, sightbased, distancebased)
- Connection handling for replays containing connetionproblems
- Transform code to more generic form to allow above supported games (less specific as with CS:GO)
- Find smoother way to count events in encounters and calculate their registration rate per match
- Equals Methods for Combatcomp, Encounter, Link
- Differ between player and entity (player = avatar in csgo ; entity = unit in age of empires controlled by a player)
