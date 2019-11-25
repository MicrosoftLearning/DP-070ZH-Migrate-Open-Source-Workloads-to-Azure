#! /bin/bash

if [ $# -lt 2 ]
then
   echo 'Usage: bash copy_adventureworks.sh adventureworks[nnn] [resource-group-name] [location]'
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
    --admin-user awadmin \
    --name $1 \
    --resource-group $2 \
    --sku-name B_Gen5_2 \
    --storage-size 5120

az postgres db create \
    --name azureadventureworks \
    --server-name $1 \
    --resource-group $2

az postgres server firewall-rule create \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0  \
    --name clientrule \
    --resource-group $2 \
    --server-name $1 
    
psql -h $1.postgres.database.azure.com -U awadmin@$1 -d postgres -f adventureworks/create_user.sql

psql -h $1.postgres.database.azure.com -U azureuser@$1 -d azureadventureworks -f adventureworks/adventureworks.sql 2> /dev/null

echo "Setup Complete"
