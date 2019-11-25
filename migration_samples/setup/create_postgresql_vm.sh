#!/bin/bash

if [ $# -lt 1 ]
then
   echo 'Usage: bash create_postgresql_vm.sh [resource-group-name] [location]'
   exit 1
fi

rgname=$1
location="$2"

if [ "$location" = "" ]
then
    location="westus"
fi

if [ $(az group exists --name $rgname) = "false" ]
then
    az group create \
        --name $rgname \
        --location $location
fi


az vm create \
    --resource-group $rgname \
    --name postgresqlvm \
    --admin-username azureuser \
    --admin-password Pa55w.rdDemo \
    --image UbuntuLTS \
    --public-ip-address-allocation static \
    --public-ip-sku Standard \
    --vnet-name postgresqlvnet \
    --nsg ""

az vm run-command invoke \
    --resource-group $rgname \
    --name postgresqlvm \
    --command-id RunShellScript \
    --scripts '

# Install dotnet core
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get -y update
sudo apt-get install dotnet-sdk-2.2=2.2.102-1

# Install PostgreSQL
sudo echo deb http://apt.postgresql.org/pub/repos/apt/ bionic-pgdg main > /etc/apt/sources.list.d/pgdg.list

sudo wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -

sudo apt-get -y update

sudo apt-get -y install postgresql-10

# Configure PostgreSQL
sudo bash << EOF
   printf "listen_addresses = \x27*\x27\nwal_level = logical\nmax_replication_slots = 5\nmax_wal_senders = 10\n" >> /etc/postgresql/10/main/postgresql.conf
   printf "host    all             all             0.0.0.0/0               md5" >> /etc/postgresql/10/main/pg_hba.conf
EOF

sudo service postgresql stop
sudo service postgresql start

# Add the azureuser role
sudo bash << EOF
su postgres << EOC
printf "create role azureuser with login;alter role azureuser createdb;alter role azureuser password \x27Pa55w.rd\x27;alter role azureuser superuser;" | psql
EOC
EOF'

az vm open-port \
    --resource-group $rgname \
    --name postgresqlvm \
    --priority 200 \
    --port '22'

az vm open-port \
    --resource-group $rgname \
    --name postgresqlvm \
    --priority 300 \
    --port '5432'

az vm list-ip-addresses \
    --resource-group $rgname\
    --name postgresqlvm | grep ipAddress

echo Setup Complete
