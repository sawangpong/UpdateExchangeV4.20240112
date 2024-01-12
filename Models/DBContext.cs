using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UpdateExchangeV4.Models
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Exchcurr> Exchcurrs { get; set; } = null!;
        public virtual DbSet<Matprice> Matprices { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("name=SQLConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exchcurr>(entity =>
            {
                entity.HasKey(e => e.Lineid);

                entity.ToTable("EXCHCURR");

                entity.Property(e => e.Lineid).HasColumnName("LINEID");

                entity.Property(e => e.Bankupdate)
                    .HasColumnType("datetime")
                    .HasColumnName("BANKUPDATE");

                entity.Property(e => e.Currency)
                    .HasMaxLength(5)
                    .HasColumnName("CURRENCY");

                entity.Property(e => e.Effectivedt)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("EFFECTIVEDT");

                entity.Property(e => e.Exchangerate)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("EXCHANGERATE");

                entity.Property(e => e.Expiredt)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("EXPIREDT");

                entity.Property(e => e.Fiscperiod)
                    .HasColumnName("FISCPERIOD")
                    .HasDefaultValueSql("(datepart(month,getdate()))");

                entity.Property(e => e.Fiscyear)
                    .HasColumnName("FISCYEAR")
                    .HasDefaultValueSql("(datepart(year,getdate()))");

                entity.Property(e => e.Lastupdate)
                    .HasColumnType("datetime")
                    .HasColumnName("LASTUPDATE");

                entity.Property(e => e.Revision).HasColumnName("REVISION");
            });

            modelBuilder.Entity<Matprice>(entity =>
            {
                entity.HasKey(e => e.Seq);

                entity.ToTable("MATPRICES");

                entity.Property(e => e.Seq).HasColumnName("SEQ");

                entity.Property(e => e.Auditdate)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("AUDITDATE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Audituser)
                    .HasMaxLength(50)
                    .HasColumnName("AUDITUSER");

                entity.Property(e => e.Costthbgram)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("COSTTHBGRAM")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Exchangerate)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("EXCHANGERATE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Exchdate)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("EXCHDATE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Fiscperiod)
                    .HasColumnName("FISCPERIOD")
                    .HasDefaultValueSql("(datepart(month,getdate()))");

                entity.Property(e => e.Fiscyear)
                    .HasColumnName("FISCYEAR")
                    .HasDefaultValueSql("(datepart(year,getdate()))");

                entity.Property(e => e.Matid).HasColumnName("MATID");

                entity.Property(e => e.Modidate)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("MODIDATE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Modiuser)
                    .HasMaxLength(50)
                    .HasColumnName("MODIUSER");

                entity.Property(e => e.Orgpriceusd)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("ORGPRICEUSD")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Pricedate)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("PRICEDATE");

                entity.Property(e => e.Pricethbgram)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("PRICETHBGRAM")
                    .HasDefaultValueSql("((0))");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
