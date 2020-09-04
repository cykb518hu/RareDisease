
using Microsoft.EntityFrameworkCore;
using RareDisease.Data.Entity;
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
}
