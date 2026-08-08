SELECT 
    b.playerID,
    b.yearID,
    b.teamID,
    b.AB,
    b.H,
    t.G as TeamGames,
    CAST(b.H AS REAL) / NULLIF(b.AB, 0) as AVG
FROM Batting b
LEFT JOIN Teams t ON b.teamID = t.teamID AND b.yearID = t.yearID AND b.lgID = t.lgID
WHERE b.playerID = 'woodsed01'
ORDER BY b.yearID;
