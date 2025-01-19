using ClientesCFA.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace ClientesCFA.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Email> Emails { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            try
            {
                if (!Database.CanConnect())
                {
                    Database.EnsureCreated();
                    Console.WriteLine("Database and tables have been created.");
                }
                CreateStoredProcedures();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization error: {ex.Message}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Address>()
                .HasOne<Person>()
                .WithMany(p => p.Addresses)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Phone>()
                .HasOne<Person>()
                .WithMany(p => p.Phones)
                .HasForeignKey(ph => ph.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Email>()
                .HasOne<Person>()
                .WithMany(p => p.Emails)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(p => p.DocumentType).IsRequired().HasMaxLength(2);
                entity.Property(p => p.DocumentNumber).IsRequired().HasMaxLength(11);
                entity.Property(p => p.Names).IsRequired().HasMaxLength(30);
                entity.Property(p => p.LastName1).IsRequired().HasMaxLength(30);
                entity.Property(p => p.LastName2).HasMaxLength(30);
                entity.Property(p => p.BirthDate).IsRequired();
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(a => a.AddressLine).IsRequired().HasMaxLength(255);
                entity.Property(a => a.AddressType).IsRequired().HasMaxLength(30);
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.Property(ph => ph.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(ph => ph.PhoneType).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Email>(entity =>
            {
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
            });
        }

        public void CreateStoredProcedures()
        {
            var checkProcedure = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'DeletePersonById')
                BEGIN
                    EXEC('
                        CREATE PROCEDURE DeletePersonById
                            @PersonId INT
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            DELETE FROM Addresses WHERE PersonId = @PersonId;
                            DELETE FROM Phones WHERE PersonId = @PersonId;
                            DELETE FROM Emails WHERE PersonId = @PersonId;

                            DELETE FROM People WHERE Id = @PersonId;
                        END;
                    ');
                END;
            ";

            try
            {
                this.Database.ExecuteSqlRaw(checkProcedure);
                Console.WriteLine("Stored procedure 'DeletePersonById' has been created or already exists.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the stored procedure: {ex.Message}");
            }
        }

    }
}




