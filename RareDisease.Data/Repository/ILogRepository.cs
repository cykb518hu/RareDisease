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
        List<OperationLog> Search(int pageIndex, int pageSize, ref int totalCount);
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

        public List<OperationLog> Search(int pageIndex, int pageSize, ref int totalCount)
        {
            var query = _context.OperationLog.AsQueryable();
            var countTask =  query.Count();
            var resultsTask = query.OrderByDescending(x=>x.CreatedOn).Skip((pageIndex-1)*pageSize).Take(pageSize).ToList();
            totalCount = countTask;

            return resultsTask;
        }
    }
}
