create table t_project_log
(
    id         integer primary key autoincrement,
    project_id integer        not null,
    time       datetime       not null default (datetime('now', 'localtime')),
    op_type    integer        not null,
    remark     nvarchar(2000) null
);

create index idx_pl_pid on t_project_log (project_id, time);