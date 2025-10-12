﻿using Microsoft.EntityFrameworkCore;
using AddressEntity = CreateInvoiceSystem.Address.Domain.Entities.Address;

namespace CreateInvoiceSystem.Persistence;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : DbContext(options)
{
    public DbSet<AddressEntity> Addresses => Set<AddressEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreateInvoiceSystemDbContext).Assembly);
    }
}
