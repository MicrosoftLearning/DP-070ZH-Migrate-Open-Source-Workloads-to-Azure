CREATE USER 'azureuser'@'%' IDENTIFIED BY 'Pa55w.rd';

GRANT ALL PRIVILEGES ON *.* TO 'azureuser'@'%';

FLUSH PRIVILEGES;

CREATE DATABASE adventureworks;