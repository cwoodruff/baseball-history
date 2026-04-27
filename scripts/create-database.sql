CREATE DATABASE baseballhistory
    ON
    ( NAME = Baseball_Data,
        FILENAME = '/var/opt/mssql/data/baseballhistory.mdf',
        SIZE = 10,
        MAXSIZE = 50,
        FILEGROWTH = 5 )
    LOG ON
    ( NAME = Baseball_Log,
        FILENAME = '/var/opt/mssql/data/baseballhistory.ldf',
        SIZE = 5MB,
        MAXSIZE = 25MB,
        FILEGROWTH = 5MB ) ;
GO

--- Replace <REPLACE_ME> with a real password
USE master
CREATE Login cwoodruff
    WITH Password='5cEZpbhz&p5i&DaA2*N68Nn4sJINd2-localonly'
GO

USE baseballhistory
CREATE USER cwoodruff FOR LOGIN cwoodruff;

ALTER ROLE db_datareader ADD MEMBER cwoodruff;
ALTER ROLE db_datawriter ADD MEMBER cwoodruff;      
GO
