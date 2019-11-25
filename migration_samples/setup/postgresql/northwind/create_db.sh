#!/bin/bash

hostname=$1
if [ "$hostname" = "" ]
then
  hostname="localhost"
fi

dropdb -U azureuser -h $hostname --if-exists northwind

createdb -U azureuser -h $hostname northwind

psql -U azureuser -h $hostname northwind < northwind.sql 2> /dev/null
