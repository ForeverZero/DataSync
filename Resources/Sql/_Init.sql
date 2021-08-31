create table t_scheme
(
    id               integer primary key autoincrement,
    database_version integer not null
);

insert into t_scheme(database_version)
values (0);
