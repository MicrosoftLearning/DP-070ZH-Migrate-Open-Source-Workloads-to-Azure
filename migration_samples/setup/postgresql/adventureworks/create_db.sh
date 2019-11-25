#!/bin/bash

# Add the azureuser role
sudo bash << EOF
su postgres << EOC
printf "create role azureuser with login;alter role azureuser createdb;alter role azureuser password \x27Pa55w.rd\x27;alter role azureuser superuser;" | psql 
EOC
EOF

## Create and populate the adventureworks database

dropdb --if-exists adventureworks

createdb adventureworks

psql adventureworks < adventureworks.sql
