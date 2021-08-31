alter table t_sys_param
    add column
        sync_time_seconds int(10) not null default 120;
alter table t_sys_param
    add column
        is_auto_start int(1) not null default 0;
    