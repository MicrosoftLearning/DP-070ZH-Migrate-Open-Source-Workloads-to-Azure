#!/bin/bash

# Add the azureuser role
sudo bash << EOF
su postgres << EOC
printf "create role azureuser with login;alter role azureuser createdb;alter role azureuser password \x27Pa55w.rd\x27;alter role azureuser superuser;" | psql 
EOC
EOF

dropdb --if-exists northwind

createdb northwind

psql northwind < northwind.sql 2> /dev/null
