using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Entity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface ILogRepository
    {

        void Add(string action, string userName = "", string message = "");
        List<OperationLogOutPut> Search(int pageIndex, int pageSize, DateTime startDate, DateTime endDate, string role, string userName, ref int totalCount);
    }

    public class GPLogRepositry: RareDiseaseGPDbContext,ILogRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GPLogRepositry> _logger;
        public GPLogRepositry(IHttpContextAccessor httpContextAccessor, ILogger<GPLogRepositry> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public void Add(string action, string userName = "", string message = "")
        {
            try
            {
                OperationLog log = new OperationLog();
                if (string.IsNullOrEmpty(userName))
                {
                    userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                }
                log.Action = action;
                log.Message = message;
                log.Guid = Guid.NewGuid().ToString();
                log.CreatedOn = DateTime.Now;
                log.CreatedBy = userName;
                dbgp.Insertable(log).ExecuteCommand();
            }
            catch(Exception ex)
            {
                _logger.LogError("添加日志失败：" + ex.ToString());
            }
        }
        public List<OperationLogOutPut> Search(int pageIndex, int pageSize, DateTime startDate, DateTime endDate, string role, string userName, ref int totalCount)
        {

            var query = dbgp.Queryable<OperationLog>();

            if (role == "admin")
            {
                query.Where(x => x.CreatedOn >= startDate && x.CreatedOn < endDate);
            }
            else
            {
                query.Where(x => x.CreatedOn >= startDate && x.CreatedOn < endDate && x.CreatedBy == userName);
            }
            var result = query.OrderBy(it => it.CreatedOn, OrderByType.Desc).ToPageList(pageIndex, pageSize, ref totalCount);

            var list = new List<OperationLogOutPut>();
            foreach (var r in result)
            {
                list.Add(new OperationLogOutPut { Action = r.Action, CreatedBy = r.CreatedBy, CreatedOn = r.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss") });
            }
            return list;
        }

    }
    public class LogRepository : ILogRepository
    {
        private readonly RareDiseaseDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogRepository> _logger;
        public LogRepository(RareDiseaseDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<LogRepository> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void Add(string action, string userName = "", string message = "")
        {
            try
            {
                OperationLog log = new OperationLog();
                if (string.IsNullOrEmpty(userName))
                {
                    userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                }
                log.Action = action;
                log.Message = message;
                log.Guid = Guid.NewGuid().ToString();
                log.CreatedOn = DateTime.Now;
                log.CreatedBy = userName;
                _context.OperationLog.Add(log);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError("记录操作日志失败：" + ex.ToString());
            }
        }

        public List<OperationLogOutPut> Search(int pageIndex, int pageSize, DateTime startDate, DateTime endDate, string role, string userName, ref int totalCount)
        {
            var query = _context.OperationLog.AsQueryable();
            var countTask = 0;
            var resultsTask = new List<OperationLog>();
            if (role == "admin")
            {
                countTask=query.Count(x => x.CreatedOn >= startDate && x.CreatedOn < endDate);
                resultsTask = query.Where(x => x.CreatedOn >= startDate && x.CreatedOn < endDate).OrderByDescending(x => x.CreatedOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                countTask = query.Count(x => x.CreatedOn >= startDate && x.CreatedOn < endDate && x.CreatedBy == userName);
                resultsTask = query.Where(x => x.CreatedOn >= startDate && x.CreatedOn < endDate && x.CreatedBy == userName).OrderByDescending(x => x.CreatedOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
             
            totalCount = countTask;
            var list = new List<OperationLogOutPut>();
            foreach(var r in resultsTask)
            {
                list.Add(new OperationLogOutPut { Action = r.Action, CreatedBy = r.CreatedBy, CreatedOn = r.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss") });
            }
            return list;
        }
    }
}
