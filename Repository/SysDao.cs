using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using DataSynchronizor.Model;
using DataSynchronizor.Repository.Base;

namespace DataSynchronizor.Repository
{
    class SysDao : BaseDao
    {
        private const string SqlSchemeExist =
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='t_scheme'";

        private const string SqlScriptNameInitScheme = "_Init";
        private const string SqlScriptPrefix = "V";

        private SysDao()
        {
        }

        private static readonly Lazy<SysDao> Instance = new Lazy<SysDao>(() => new SysDao());

        public static SysDao GetInstance => Instance.Value;

        public void InitDb()
        {
            Logger.Info("开始初始化数据库");
            InitTable();
            Logger.Info("数据库初始化完成");
        }

        private void InitTable()
        {
            InitScheme();
            var scheme = DbCtx.Schemes.Single();
            if (scheme == null)
            {
                MessageBox.Show("数据库初始化失败");
                return;
            }

            UpdateDatabase(scheme);
        }

        private void UpdateDatabase(Scheme scheme)
        {
            var curDbVersion = scheme.DatabaseVersion;
            Logger.Info($"当前数据库版本{curDbVersion}");
            var nextVer = curDbVersion;
            do
            {
                nextVer++;
                var script = LoadSqlStatement($"{SqlScriptPrefix}{nextVer}");
                if (script == string.Empty)
                {
                    return;
                }

                Logger.Info($"新的数据库版本{nextVer}");
                // 开始事务, 执行脚本
                var trx = DbCtx.Database.BeginTransaction();
                Logger.Debug($"执行{nextVer}版本Sql脚本: \n{script}");
                DbCtx.Database.ExecuteSqlCommand(script);
                scheme.DatabaseVersion = nextVer;
                DbCtx.SaveChanges();
                trx.Commit();
                Logger.Info($"数据库已升级到新版本: {nextVer}");
            } while (true);
        }

        private void InitScheme()
        {
            var schemeExist = DbCtx.Database.SqlQuery<bool>(SqlSchemeExist).ToList()[0];
            if (schemeExist)
            {
                return;
            }

            Logger.Info("Sceme不存在, 初始化Scheme");
            var sqlInitScheme = LoadSqlStatement(SqlScriptNameInitScheme);
            DbCtx.Database.ExecuteSqlCommand(sqlInitScheme);
            MessageBox.Show("数据库已创建");
        }

        private string LoadSqlStatement(string sqlScriptName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"DataSynchronizor.Resources.Sql.{sqlScriptName}.sql");
            if (stream == null)
            {
                return string.Empty;
            }

            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}