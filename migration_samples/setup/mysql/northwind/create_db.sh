#!/bin/sh

hostname=$1
if [ "$hostname" = "" ]
then
  hostname="localhost"
fi

# sudo mysql -h $hostname < create_user.sql
mysql -h $hostname --user=azureuser --password=Pa55w.rd < northwind.sql 2> /dev/null
