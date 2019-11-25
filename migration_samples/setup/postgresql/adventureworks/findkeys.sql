SELECT concat('alter table ', table_schema, '.', table_name, ' ', ' DROP CONSTRAINT "', constraint_name, '";')
FROM information_schema.table_constraints
WHERE constraint_type = 'FOREIGN KEY';
