using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Safepot.DataAccess
{
    public class SfpDataRepository<T> : DbFactoryBase, ISfpDataRepository<T> where T : class
    {
        private readonly SafepotDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly string _encryptionkey;
        private readonly IConfiguration _config;

        public SfpDataRepository(IConfiguration config, SafepotDbContext context)
            : base(config)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _config = config;
            _encryptionkey = _config["EncryptionKey"] ?? ""; //"VtbM/yjSA2Q=";
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
            _context.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            try
            {
                EncryptProperties(new List<T> { entity });
                _context.Add(entity);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                await CreateAsync(entity);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync(IEnumerable<T> entities)
        {
            foreach (T entity in entities.Distinct())
            {
                await DeleteAsync(entity);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            foreach (T entity in entities.Distinct())
            {
                await DeleteAsync(entity);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            EncryptProperties(new List<T> { entity });
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
        }

        public async Task ClearData()
        {
            await DbExecuteClearDataAsync();
        }

        private Dictionary<string, object> GetDictionaryColumnNameValues(T entity)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            Type attrType = typeof(T);
            foreach (PropertyInfo property in attrType.GetProperties())
            {
                if (property.Name != "Id")
                {
                    object value = property.GetValue(entity) ?? "";
                    if (value != null)
                    {
                        keyValues.Add(GetColumnName(property), value);
                    }
                }
            }
            return keyValues;
        }

        private string GetSqlQuery(string[] keys, string tableName = "")
        {
            return $"INSERT INTO {GetTableName(tableName)} ({GetKeysString(keys)}) output inserted.Id VALUES ({GetValuesString(keys.Length)})";
        }

        private object GetValuesString(int count)
        {
            int[] indexes = new int[count];
            for (int i = 0; i < indexes.Length; i++)
            {
                indexes[i] = i + 1;
            }
            return $"@{string.Join(", @", indexes)}";
        }

        private object GetKeysString(string[] keys)
        {
            return string.Join(", ", keys.ToArray());
        }

        private string GetTableName(string tableName)
        {
            Type attrType = typeof(T);
            return string.IsNullOrEmpty(tableName) ? attrType.Name : tableName;
        }

        private string GetColumnName(PropertyInfo property)
        {
            return property.Name;
        }

        public void Edit(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate).AsNoTracking();
            return query;
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IEnumerable<T> result = new List<T>();
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter).AsNoTracking();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsNoTracking();
            }
            result = query.ToList();
            if (result == null)
                result = new List<T>();
            else
            {
                DecryptProperties(result.ToList());
            }
            return result;
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            try
            {
                IEnumerable<T> result = new List<T>();
                IQueryable <T> query = _dbSet;
                if (filter != null)
                {
                    query = query.Where(filter).AsNoTracking();
                }
                if (orderBy != null)
                {
                    result = await orderBy(query).AsNoTracking().ToListAsync();
                }
                result = await query.ToListAsync();
                if (result == null || result.Count() == 0)
                    result = new List<T>();
                else
                {
                    DecryptProperties(result.ToList());
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;
                if (filter != null)
                {
                    query = query.Where(filter).AsNoTracking();
                }
                if (orderBy != null)
                {
                    return await orderBy(query).AsNoTracking().FirstAsync();
                }
                T result = await query.FirstAsync();
                if (result == null)
                    result = default(T);
                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }

        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }

        public void EncryptProperties(List<T> list)
        {
            try
            {
                var props = typeof(T).GetProperties();
                foreach (T item in list)
                {
                    foreach (var prop in props)
                    {
                        if(prop.PropertyType == typeof(string))
                        {
                            var propVal = prop.GetValue(item, null);
                            if(propVal != null && Convert.ToString(propVal) != "" && Convert.ToString(propVal) != "string")
                              prop.SetValue(item, EncryptionHelper.Encrypt(propVal.ToString() ?? "", _encryptionkey), null);
                            else
                                prop.SetValue(item, "", null);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void DecryptProperties(List<T> list)
        {
            try
            {
                var props = typeof(T).GetProperties();
                foreach (T item in list)
                {
                    foreach (var prop in props)
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            var propVal = prop.GetValue(item, null);
                            if (propVal != null && Convert.ToString(propVal) != "")
                                prop.SetValue(item, EncryptionHelper.Decrypt(propVal.ToString() ?? "", _encryptionkey), null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, string>? GetParameterDecryptValues(Dictionary<string,string> parameters)
        {
            try
            {
                if(parameters!=null && parameters.Count() > 0)
                {
                    foreach(var param in parameters)
                    {
                        parameters[param.Key] = EncryptionHelper.Encrypt(param.Value, _encryptionkey);
                    }
                }
                return parameters;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
