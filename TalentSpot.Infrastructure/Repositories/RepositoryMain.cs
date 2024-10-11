//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text.Json.Serialization;
//using System.Threading;
//using System.Threading.Tasks;
//using Dapper;
//using Microsoft.EntityFrameworkCore;
//using Netlog.Crm.Core.Entities;
//using Netlog.Crm.Core.Helpers;
//using Netlog.Crm.Core.Interfaces.RepositoryInterfaces;

//namespace Netlog.Crm.Repository.Concrete.EntityFramework
//{
//    public class RepositoryMain : IRepositoryMain
//    {
//        private readonly ApplicationDbContext _dbContext;

//        public RepositoryMain(ApplicationDbContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        public virtual async Task<int> Add<T>(T entity) where T : class
//        {
//            _dbContext.Set<T>().Add(entity);
//            var i = await _dbContext.SaveChangesAsync();
//            _dbContext.Entry(entity).State = EntityState.Detached;

//            _dbContext.ChangeTracker.Entries()
//                    .Where(x => x.State == EntityState.Unchanged)
//                    .ToList()
//                    .ForEach(x => { x.State = EntityState.Detached; });
//            return i;
//        }

//        public virtual async Task<int> AddRange<T>(List<T> entities) where T : class
//        {
//            _dbContext.Set<T>().AddRange(entities);
//            var i = await _dbContext.SaveChangesAsync();
//            foreach (var entity in entities)
//                _dbContext.Entry(entity).State = EntityState.Detached;

//            _dbContext.ChangeTracker.Entries()
//                    .Where(x => x.State == EntityState.Unchanged)
//                    .ToList()
//                    .ForEach(x => { x.State = EntityState.Detached; });
//            return i;
//        }

//        public virtual async Task<int> Delete<T>(T entity) where T : class
//        {
//            Guid id = GetKey(entity);
//            var delEntity = await this.GetById<T>(id);

//            if (delEntity == null || delEntity == default(T))
//                return 0;

//            if (_dbContext.Entry(delEntity).Metadata.FindProperty(nameof(EntityMain.DeleteFlag)) == null
//                || _dbContext.Entry(delEntity).Metadata.FindProperty(nameof(EntityMain.ModifiedBy)) == null)
//                return 0;

//            _dbContext.Entry(delEntity).Property(nameof(EntityMain.ModifiedBy)).CurrentValue = _dbContext.Entry(entity).Property(nameof(EntityMain.ModifiedBy)).CurrentValue;
//            _dbContext.Entry(delEntity).Property(nameof(EntityMain.ModifiedBy)).IsModified = true;

//            _dbContext.Entry(delEntity).Property(nameof(EntityMain.DeleteFlag)).CurrentValue = true;
//            _dbContext.Entry(delEntity).Property(nameof(EntityMain.DeleteFlag)).IsModified = true;

//            var i = await _dbContext.SaveChangesAsync();
//            _dbContext.Entry(delEntity).State = EntityState.Detached;
//            return i;
//        }

//        public virtual async Task<int> DeleteRange<T>(List<T> entities) where T : class
//        {
//            foreach (var entity in entities)
//            {
//                Guid id = GetKey(entity);
//                var delEntity = await this.GetById<T>(id);

//                if (delEntity == null || delEntity == default(T))
//                    return 0;

//                if (_dbContext.Entry(delEntity).Metadata.FindProperty(nameof(EntityMain.DeleteFlag)) == null
//                    || _dbContext.Entry(delEntity).Metadata.FindProperty(nameof(EntityMain.ModifiedBy)) == null)
//                    return 0;

//                _dbContext.Entry(delEntity).Property(nameof(EntityMain.ModifiedBy)).CurrentValue = _dbContext.Entry(entity).Property(nameof(EntityMain.ModifiedBy)).CurrentValue;
//                _dbContext.Entry(delEntity).Property(nameof(EntityMain.ModifiedBy)).IsModified = true;

//                _dbContext.Entry(delEntity).Property(nameof(EntityMain.DeleteFlag)).CurrentValue = true;
//                _dbContext.Entry(delEntity).Property(nameof(EntityMain.DeleteFlag)).IsModified = true;
//            }
//            var i = await _dbContext.SaveChangesAsync();
//            foreach (var entity in entities)
//                _dbContext.Entry(entity).State = EntityState.Detached;
//            return i;
//        }

//        public virtual async Task<int> Execute(string query)
//        {
//            return await _dbContext.Database.ExecuteSqlRawAsync(query);
//        }

//        public virtual async Task<int> Edit<T>(T entity) where T : class
//        {
//            //Güncelleyen kişi
//            var userId = entity.GetType().GetProperty(nameof(EntityMain.ModifiedBy))?.GetValue(entity);

//            Guid id = GetKey(entity);

//            var dbEntity = await _dbContext.FindAsync<T>(id); //Değiştirilmeden önceki kayıt çekiliyor
//            var dbEntry = _dbContext.Entry(dbEntity);

//            dbEntry.CurrentValues.SetValues(entity); //Yeni gelen değerler ile güncelleniyor

//            var navigations = _dbContext.Model.FindEntityType(typeof(T)).GetNavigations() //Json ignore işaretli olmayan collectionlar çekiliyor
//                .Where(property =>
//                typeof(T).GetProperty(property.Name).GetCustomAttribute<JsonIgnoreAttribute>() == null &&
//                property.IsCollection);

//            foreach (var property in navigations) //her navigation için yenisi varsa ekleniyor, var olan güncelleniyor, öncekine göre kaydı olmayan siliniyor
//            {
//                var propertyName = property.Name;
//                var dbItemsEntry = dbEntry.Collection(propertyName);
//                var accessor = dbItemsEntry.Metadata.GetCollectionAccessor();

//                await dbItemsEntry.LoadAsync(); //Veritabanından önceki kaydı çekiliyor

//                var dbItemsMap = new Dictionary<Guid, object>();
//                foreach (var item in (System.Collections.IEnumerable)dbItemsEntry.CurrentValue) //her bir kayıt guid,object olarak dictionary'ye ekleniyor (dbItemsMap)
//                {
//                    var keyName = _dbContext.Model.FindEntityType(item.GetType()).FindPrimaryKey().Properties.Select(x => x.Name).Single();
//                    var key = (Guid)item.GetType().GetProperty(keyName)?.GetValue(item, null);

//                    item.GetType().GetProperty(nameof(EntityMain.ModifiedOn))?.SetValue(item, DateTime.UtcNow);
//                    item.GetType().GetProperty(nameof(EntityMain.ModifiedBy))?.SetValue(item, userId);
//                    dbItemsMap.Add(key, item);
//                }

//                var items = (IEnumerable<EntityMain>)accessor.GetOrCreate(entity, false); //Güncellenen kayıtlar Collection'a ekleniyor

//                foreach (var item in items) //Güncel kayıtlar dönülerek, önceki kayıtlarda aranıyor
//                {
//                    var keyName = _dbContext.Model.FindEntityType(item.GetType()).FindPrimaryKey().Properties.Select(x => x.Name).Single();
//                    var key = (Guid)item.GetType().GetProperty(keyName)?.GetValue(item, null);

//                    if (!dbItemsMap.TryGetValue(key, out var oldItem)) //Güncel kayıtlar dbItemsMap'te yok ise entityState added olarak ekleniyor
//                    {
//                        item.GetType().GetProperty(nameof(EntityMain.CreatedOn))?.SetValue(item, DateTime.UtcNow);
//                        item.GetType().GetProperty(nameof(EntityMain.CreatedBy))?.SetValue(item, userId);
//                        _dbContext.Entry(item).State = EntityState.Added;

//                        accessor.Add(dbEntity, item, false);
//                    }
//                    else  //Daha önceden var olan kayıtlar güncelleniyor
//                    {
//                        item.GetType().GetProperty(nameof(EntityMain.CreatedBy))?.SetValue(item, oldItem.GetType().GetProperty(nameof(EntityMain.CreatedBy)).GetValue(oldItem, null));
//                        item.GetType().GetProperty(nameof(EntityMain.CreatedOn))?.SetValue(item, oldItem.GetType().GetProperty(nameof(EntityMain.CreatedOn)).GetValue(oldItem, null));
//                        item.GetType().GetProperty(nameof(EntityMain.Code))?.SetValue(item, oldItem.GetType().GetProperty(nameof(EntityMain.Code)).GetValue(oldItem, null));
//                        _dbContext.Entry(oldItem).CurrentValues.SetValues(item);
//                        oldItem.GetType().GetProperty(nameof(EntityMain.ModifiedOn))?.SetValue(oldItem, DateTime.UtcNow);
//                        oldItem.GetType().GetProperty(nameof(EntityMain.ModifiedBy))?.SetValue(oldItem, userId);
//                        dbItemsMap.Remove(key);
//                    }
//                }

//                //foreach (var oldItem in dbItemsMap.Values) //güncelleme sonrası dictionary'de kalan kayıtlar siliniyor (dbItemsMap)
//                //    accessor.Remove(dbEntity, oldItem);

//                foreach (var oldItem in dbItemsMap.Values)//güncelleme sonrası dictionary'de kalan kayıtlar siliniyor (dbItemsMap)
//                {
//                    oldItem.GetType().GetProperty(nameof(EntityMain.ModifiedOn))?.SetValue(oldItem, DateTime.UtcNow);
//                    oldItem.GetType().GetProperty(nameof(EntityMain.ModifiedBy))?.SetValue(oldItem, userId);
//                    oldItem.GetType().GetProperty(nameof(EntityMain.DeleteFlag))?.SetValue(oldItem, true);
//                }
//            }

//            int i = await _dbContext.SaveChangesAsync();
//            _dbContext.Entry(entity).State = EntityState.Detached;

//            _dbContext.ChangeTracker.Entries()
//                    .Where(x => x.State == EntityState.Unchanged)
//                    .ToList()
//                    .ForEach(x => { x.State = EntityState.Detached; });

//            return i;
//        }

//        public virtual Guid GetKey<T>(T entity)
//        {
//            var keyName = _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
//                .Select(x => x.Name).Single();

//            return (Guid)entity.GetType().GetProperty(keyName)?.GetValue(entity, null);
//        }

//        public virtual async Task<T> GetById<T>(Guid id, bool eager = false) where T : class
//        {
//            var entity = await _dbContext.Set<T>()
//                    .FindAsync(id);

//            if (entity == null)
//                return entity;

//            var navigations = new List<Task>();
//            if (eager)//JsonIgnore harici olanlar dolduruluyor
//                foreach (var property in _dbContext.Model.FindEntityType(typeof(T)).GetNavigations())
//                {
//                    PropertyInfo propertyInfo = typeof(T).GetProperty(property.Name);
//                    var jsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
//                    if (jsonIgnore == null)
//                        navigations.Add(_dbContext.Entry(entity).Navigation(property.Name).LoadAsync());
//                }

//            Task.WaitAll(navigations.ToArray());
//            _dbContext.Entry(entity).State = EntityState.Detached;
//            return entity;
//        }

//        public virtual async Task<int> UpdateFieldsSave<T>(T entity, params Expression<Func<T, object>>[] includeProperties) where T : class
//        {
//            var dbEntry = _dbContext.Entry(entity);
//            foreach (var includeProperty in includeProperties)
//            {
//                dbEntry.Property(includeProperty).IsModified = true;
//            }
//            int i = await _dbContext.SaveChangesAsync();
//            _dbContext.Entry(entity).State = EntityState.Detached;
//            return i;
//        }

//        public virtual async Task<int> BulkUpdateFieldsSave<T>(List<T> entities, params Expression<Func<T, object>>[] includeProperties) where T : class
//        {
//            foreach (var entity in entities)
//            {
//                var dbEntry = _dbContext.Entry(entity);
//                foreach (var includeProperty in includeProperties)
//                {
//                    dbEntry.Property(includeProperty).IsModified = true;
//                }
//            }
//            int i = await _dbContext.SaveChangesAsync();

//            foreach (var entity in entities)
//                _dbContext.Entry(entity).State = EntityState.Detached;

//            return i;
//        }

//        public virtual async Task<IEnumerable<T>> List<T>(bool eager = false, IEnumerable<string> includes = null) where T : class
//        {
//            var query = QueryWithInclude<T>(eager);

//            if (includes != null)
//                foreach (var include in includes)
//                    query = query.Include(include);

//            var result = await Task.Run(() =>
//                query.AsEnumerable()
//                );
//            return result;
//        }

//        public virtual async Task<IEnumerable<T>> List<T>(Expression<Func<T, bool>> predicate, bool eager = false, IEnumerable<string> includes = null) where T : class
//        {
//            var query = QueryWithInclude<T>(eager);

//            if (predicate != null)
//                query = query.Where(predicate);

//            if (includes != null)
//                foreach (var include in includes)
//                    query = query.Include(include);

//            var result = await Task.Run(() =>
//                    query.AsEnumerable()
//                 );
//            return result;
//        }

//        public virtual async Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class
//        {
//            var query = _dbContext.Set<T>()
//                .AsQueryable()
//                .AsNoTracking()
//                .Where(predicate);

//            var result = await query.CountAsync();

//            return result;
//        }

//        internal virtual IQueryable<T> QueryWithInclude<T>(bool eager = false) where T : class
//        {
//            var query = _dbContext.Set<T>().AsQueryable().AsNoTracking();
//            if (eager)
//                foreach (var property in _dbContext.Model.FindEntityType(typeof(T)).GetNavigations())
//                {
//                    PropertyInfo propertyInfo = typeof(T).GetProperty(property.Name);
//                    var jsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
//                    if (jsonIgnore == null)
//                        query = query.Include(property.Name);
//                }
//            return query;
//        }

//        public virtual async Task<IEnumerable<T>> List<T>(Expression<Func<T, T>> selector, Expression<Func<T, bool>> predicate = null, Expression<Func<T, object>> orderSelector = null, bool orderAsc = false) where T : class
//        {
//            var query = _dbContext.Set<T>().AsQueryable().AsNoTracking();

//            if (null != predicate)
//                query = query.Where(predicate);

//            if (null != selector)
//                query = query.Select(selector);

//            if (null != orderSelector)
//                query = orderAsc ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);

//            var result = await query.ToListAsync();

//            return result;
//        }

//        public virtual async Task<IPagedResult<T>> List<T>(Expression<Func<T, T>> selector, int page, int pageSize, Expression<Func<T, bool>> predicate = null, Expression<Func<T, object>> orderSelector = null, bool orderAsc = false) where T : class
//        {
//            var query = _dbContext.Set<T>().AsQueryable().AsNoTracking();

//            if (null != predicate)
//                query = query.Where(predicate);

//            if (null != selector)
//                query = query.Select(selector);

//            var rowCount = await query.CountAsync();

//            if (null != orderSelector)
//                query = orderAsc ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);

//            var result = await query.GetPagedAsync(page, pageSize, rowCount);

//            return result;
//        }

//        private async Task<IPagedResult<T>> List<T>(int page, int pageSize, Expression<Func<T, bool>> predicate = null, IEnumerable<(Expression<Func<T, object>> orderSelector, bool orderAsc)> orders = null,
//                                            Expression<Func<T, T>> selector = null, bool eager = false, IEnumerable<string> includes = null) where T : class
//        {
//            var query = QueryWithInclude<T>(eager);

//            if (predicate != null)
//                query = query.Where(predicate);

//            if (null != selector)
//                query = query.Select(selector);

//            if (includes != null && includes.Any())
//                foreach (var include in includes)
//                    query = query.Include(include);

//            var rowCount = await query.CountAsync();

//            if (orders != null && orders.Any())
//            {
//                var orderedQuery = orders.First().orderAsc ? query.OrderBy(orders.First().orderSelector) : query.OrderByDescending(orders.First().orderSelector);

//                foreach (var order in orders.Skip(1))
//                    orderedQuery = order.orderAsc ? orderedQuery.ThenBy(order.orderSelector) : orderedQuery.ThenByDescending(order.orderSelector);

//                query = orderedQuery;
//            }

//            var result = await query.GetPagedAsync(page, pageSize, rowCount);

//            return result;
//        }

//        public virtual async Task<IPagedResult<T>> List<T>(Expression<Func<T, T>> selector, int page, int pageSize, Expression<Func<T, bool>> predicate = null, IEnumerable<(Expression<Func<T, object>> orderSelector, bool orderAsc)> orders = null) where T : class
//        {
//            return await List(page, pageSize, predicate, orders, selector, false, null);
//        }

//        public virtual async Task<IPagedResult<T>> List<T>(int page, int pageSize, Expression<Func<T, bool>> predicate = null, IEnumerable<(Expression<Func<T, object>> orderSelector, bool orderAsc)> orders = null, bool eager = false) where T : class
//        {
//            return await List(page, pageSize, predicate, orders, null, eager);
//        }

//        public virtual async Task<IPagedResult<T>> List<T>(int page, int pageSize, Expression<Func<T, bool>> predicate = null, IEnumerable<(Expression<Func<T, object>> orderSelector, bool orderAsc)> orders = null, bool eager = false, IEnumerable<string> includes = null) where T : class
//        {
//            return await List(page, pageSize, predicate, orders, null, eager, includes);
//        }

//        /// <summary>
//        /// Sadece 'CREATE TEMP TABLE' vb. database üzerinde değişiklik yapan raporlar için kullanılır.
//        /// Aksi takdirde <see cref="ReportRepository"/> üzerindeki <see cref="ReportRepository.GetReportList"/> metodu kullanılır.
//        /// </summary>
//        /// <returns></returns>
//        public virtual async Task<IEnumerable<T>> GetReport<T>(string functionName, object parameters = null) where T : class
//        {
//            var conn = _dbContext.Database.GetDbConnection();
//            if (conn.State == ConnectionState.Closed)
//                await conn.OpenAsync(CancellationToken.None);

//            var result = await conn.QueryAsync<T>("SELECT * FROM " + functionName + "", parameters, commandType: CommandType.Text, commandTimeout: null);
//            return result;
//        }

//        internal IEnumerable<string> PopulateTexts(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return new List<string>().ToHashSet();

//            var listText = new List<string>();
//            var texts = new List<string>();
//            var cultureTR = new System.Globalization.CultureInfo("tr-TR", false);

//            text = text.Trim();
//            var textLatin = ChangeToLatin(text);
//            var textClean = string.Concat(text.Where(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)));
//            var textLatinClean = string.Concat(textLatin.Where(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)));
//            listText.Add(text);
//            listText.Add(textLatin);
//            listText.Add(textClean);
//            listText.Add(textLatinClean);

//            if (textClean.Length < 1 || textLatinClean.Length < 1)
//                return texts.ToHashSet();

//            foreach (var item in listText)
//            {
//                texts.Add(item);
//                texts.Add(item.ToUpper());
//                texts.Add(item.ToUpper(cultureTR));
//                texts.Add(item.ToLower());
//                texts.Add(item.ToLower(cultureTR));
//                texts.Add(System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(item));
//                texts.Add(cultureTR.TextInfo.ToTitleCase(item));
//            }
//            return texts.ToHashSet().ToList();
//        }

//        internal string ChangeToLatin(string text)
//        {
//            return text
//                    .Replace("ı", "i")
//                    .Replace("ö", "o")
//                    .Replace("ü", "u")
//                    .Replace("İ", "I")
//                    .Replace("Ö", "O")
//                    .Replace("Ü", "U")
//                    .Replace("ç", "c")
//                    .Replace("ğ", "g")
//                    .Replace("ş", "s")
//                    .Replace("Ç", "C")
//                    .Replace("Ğ", "G")
//                    .Replace("Ş", "S");
//        }
//    }
//}