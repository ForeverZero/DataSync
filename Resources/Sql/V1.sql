create table t_sys_param
(
    id          integer primary key autoincrement,
    sync_folder nvarchar(500) null
);

insert into t_sys_param(sync_folder)
values (null);

create table t_project
(
    id              integer primary key autoincrement,
    name            nvarchar(50)  not null,
    folder          nvarchar(500) null,
    last_sync_time  datetime      null,
    monitor_process nvarchar(100) null,
    create_time     datetime      not null default (datetime('now', 'localtime'))
);

create unique index idx_prj_name on t_project (name);
create unique index idx_prj_fdr on t_project (folder);