
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RareDisease.Data.Entity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RareDisease.Data
{
    public class RareDiseaseDbContext : DbContext
    {
        public RareDiseaseDbContext()
        {
        }
        public RareDiseaseDbContext(DbContextOptions<RareDiseaseDbContext> options)
           : base(options)
        {
        }

        public virtual DbSet<OperationLog> OperationLog { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<OperationLog>(entity =>
        //    {
        //        entity.ToTable("OperationLog");
        //        entity.Property(e => e.Guid).HasColumnName("DiseaseID");
        //    });

        //}
    }
    public class RareDiseaseGPDbContext
    {
        public static string NLP_ConnectionString { get; set; }

        public SqlSugarClient dbgp;//用来处理事务多表查询和复杂的操作 
        public RareDiseaseGPDbContext()
        {
            dbgp = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = NLP_ConnectionString,
                DbType = DbType.PostgreSQL,
                //DbType = DbType.SqlServer,
                InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样
            });
        }
    }
}
