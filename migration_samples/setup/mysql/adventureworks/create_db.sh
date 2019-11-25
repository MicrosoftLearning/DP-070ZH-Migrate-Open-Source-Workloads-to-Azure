#! /bin/sh

hostname=$1
if [ "$hostname" = "" ]
then
  hostname="localhost"
fi

# sudo -h $hostname mysql < create_user.sql
mysql -h $hostname --user=azureuser --password=Pa55w.rd -p adventureworks < adventureworks.sql 2> /dev/null
