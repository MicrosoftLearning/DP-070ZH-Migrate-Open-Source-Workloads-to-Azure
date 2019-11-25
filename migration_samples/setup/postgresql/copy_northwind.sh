#! /bin/bash

if [ $# -lt 2 ]
then
   echo 'Usage: bash copy_northwind.sh northwind[nnn] [resource-group-name] [location]'
   exit 1
fi

location="$3"
if [ "$location" = "" ]
then
    location="westus"
fi

if [ $(az group exists --name $2) = "false" ]
then
    az group create \
        --name $2 \
        --location $3
fi

az postgres server create \
    --admin-password 'Pa55w.rdDemo' \
    --admin-user northwindadmin \
    --name $1 \
    --resource-group $2 \
    --sku-name B_Gen5_2 \
    --storage-size 5120

az postgres db create \
    --name azurenorthwind \
    --server-name $1 \
    --resource-group $2

az postgres server firewall-rule create \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0  \
    --name clientrule \
    --resource-group $2 \
    --server-name $1 
    
psql -h $1.postgres.database.azure.com -U northwindadmin@$1 -d azurenorthwind -f northwind/northwind.sql

echo "Setup Complete"
