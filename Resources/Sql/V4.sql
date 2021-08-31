alter table t_sys_param
    add start_hide int(1) default 0 not null;

alter table t_sys_param
    add start_message int(1) default 1 not null;

alter table t_sys_param
    add conflict_message int(1) default 1 not null;