using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RareDisease.Data.Entity;
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

    public class LogRepository : ILogRepository
    {
        private readonly RareDiseaseDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LogRepository(RareDiseaseDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Add(string action, string userName = "", string message = "")
        {
            OperationLog log = new OperationLog();
            if(string.IsNullOrEmpty(userName))
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

        public List<OperationLogOutPut> Search(int pageIndex, int pageSize, DateTime startDate, DateTime endDate, string role, string userName, ref int totalCount)
        {
            var query = _context.OperationLog.AsQueryable();
            var countTask = 0;
            var resultsTask = new List<OperationLog>();
            if (role == "admin")
            {
                countTask=query.Count(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate);
                resultsTask = query.Where(x => x.CreatedOn > startDate && x.CreatedOn <= endDate).OrderByDescending(x => x.CreatedOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                countTask = query.Count(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && x.CreatedBy == userName);
                resultsTask = query.Where(x => x.CreatedOn > startDate && x.CreatedOn <= endDate && x.CreatedBy == userName).OrderByDescending(x => x.CreatedOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
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
