-- Drop existing tables to allow clean migration
DROP TABLE IF EXISTS "Attendances" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;
DROP TABLE IF EXISTS "Shifts" CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;
