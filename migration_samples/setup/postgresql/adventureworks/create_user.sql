create role azureuser with login;
alter role azureuser createdb;
alter role azureuser password 'Pa55w.rd';
grant all privileges on database azureadventureworks to azureuser;
grant azure_pg_admin to azureuser;