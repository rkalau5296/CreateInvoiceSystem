﻿using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Abstractions.DbContext;

public interface ICreateInvoiceSystemDbContext
{
    DbSet<T> Set<T>() where T : class;
}
