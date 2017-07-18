using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleBank.DataService.Data;
using SimpleBank.DataService.Entity;

namespace SimpleBank.DataService.Migrations
{
    [DbContext(typeof(SimpleBankContext))]
    partial class SimpleBankContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleBank.DataService.Entity.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<decimal>("Balance");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Email");

                    b.Property<string>("FullName");

                    b.Property<string>("Number");

                    b.Property<string>("Password");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("ID");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Number")
                        .IsUnique();

                    b.ToTable("Account");
                });

            modelBuilder.Entity("SimpleBank.DataService.Entity.Transaction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int?>("FromAccountID");

                    b.Property<int?>("ToAccountID");

                    b.Property<int>("Type");

                    b.HasKey("ID");

                    b.HasIndex("FromAccountID");

                    b.HasIndex("ToAccountID");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("SimpleBank.DataService.Entity.Transaction", b =>
                {
                    b.HasOne("SimpleBank.DataService.Entity.Account", "FromAccount")
                        .WithMany()
                        .HasForeignKey("FromAccountID");

                    b.HasOne("SimpleBank.DataService.Entity.Account", "ToAccount")
                        .WithMany()
                        .HasForeignKey("ToAccountID");
                });
        }
    }
}
