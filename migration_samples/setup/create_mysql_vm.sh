#!/bin/bash

if [ $# -lt 1 ]
then
   echo 'Usage: bash create_mysql_vm.sh [resource-group-name] [location]'
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
    --name mysqlvm \
    --admin-username azureuser \
    --admin-password Pa55w.rdDemo \
    --image UbuntuLTS \
    --public-ip-address-allocation static \
    --public-ip-sku Standard \
    --vnet-name mysqlvnet \
    --nsg ""

az vm run-command invoke \
    --resource-group $rgname \
    --name mysqlvm \
    --command-id RunShellScript \
    --scripts '

# Install dotnet core
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get -y update
sudo apt-get install dotnet-sdk-2.2=2.2.102-1

# Install MySQL 
sudo apt-get -y update
export DEBIAN_FRONTEND=noninteractive  
sudo -E apt-get -q -y install mysql-server

sudo bash << EOF
    echo "bind-address=0.0.0.0" >> /etc/mysql/mysql.conf.d/mysqld.cnf
    echo "log-bin" >> /etc/mysql/mysql.conf.d/mysqld.cnf
    echo "server-id=99" >> /etc/mysql/mysql.conf.d/mysqld.cnf
EOF
sudo service mysql stop
sudo service mysql start 

sudo mysqladmin -u root password Pa55w.rd

# Add the azureuser role
sudo bash << EOF
printf "CREATE USER \x27azureuser\x27@\x27%%\x27 IDENTIFIED BY \x27Pa55w.rd\x27;GRANT ALL PRIVILEGES ON \x2A.\x2A TO \x27azureuser\x27@\x27%%\x27;FLUSH PRIVILEGES;" | mysql
EOF
'

az vm open-port \
    --resource-group $rgname \
    --name mysqlvm \
    --priority 200 \
    --port '22'

az vm open-port \
    --resource-group $rgname \
    --name mysqlvm \
    --priority 300 \
    --port '3306'

az vm list-ip-addresses \
    --resource-group $rgname\
    --name mysqlvm | grep ipAddress


echo Setup Complete
