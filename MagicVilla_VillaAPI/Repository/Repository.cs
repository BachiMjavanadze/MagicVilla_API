﻿using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MagicVilla_VillaAPI.Repository.IRepository;

namespace MagicVilla_VillaAPI.Repository;

public class Repository<T> : IRepository<T> where T : class
{
  private readonly ApplicationDbContext _db;
  internal DbSet<T> dbSet;

  public Repository(ApplicationDbContext db)
  {
    _db = db;
    this.dbSet = _db.Set<T>();
  }

  public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true)
  {
    //IQueryable<T> query = _db.Villas;
    IQueryable<T> query = dbSet;

    if (!tracked)
    {
      query = query.AsNoTracking();
    }

    if (filter != null)
    {
      query = query.Where(filter);
    }

    return await query.FirstOrDefaultAsync();
  }

  public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
  {
    //IQueryable<Villa> query = _db.Villas;
    IQueryable<T> query = dbSet;

    if (filter != null)
    {
      query = query.Where(filter);
    }

    return await query.ToListAsync();
  }

  public async Task CreateAsync(T entity)
  {
    //await _db.Villas.AddAsync(entity);
    await dbSet.AddAsync(entity);
    await SaveAsync();
  }

  public async Task RemoveAsync(T entity)
  {
    //_db.Villas.Remove(entity);
    dbSet.Remove(entity);
    await SaveAsync();
  }

  public async Task SaveAsync()
  {
    await _db.SaveChangesAsync();
  }
}
