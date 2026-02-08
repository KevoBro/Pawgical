Names of members:
Kevin Huynh
Joshua Perras
Tuong Vi Nguyen

  The game revolves around the player working to survive an endless pursuit of forest animals. However, they are equipped with magic spells, 
which they can cast through drawing the appropriate shapes and then firing away. Each spell has its own unique properties, 
and so it is up to the player to make strategic use of their arsenal to clear the waves of enemies as long as they can. 

  Many of our problems went into debugging and interpreting the code generated from Claude, since it was dealing with a language and area of game development we weren't largely familiar with. 
One of our primary features is the ability for the game to recognize the shapes that the player draws as part of their spell cast.
However, a large chunk of our time was spent figuring out why the pattern recognition results would come out messy at times.
We eventually concluded that the omission of the Z-axis values had likely tampered with how our 3D drawings projected into a 2D plane would be interpreted.
Additionally, we believe that our data set is likely still being tampered, with by where the player was in a 3D space at the time of recording coordinates into the data set,
making it give weird results in spaces/areas where we did not record our shape drawings.


Built using Visual Studio Code, Unity, and Claude.



