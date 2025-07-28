using Aibu.InternshipAutomation.Data.Base;
using Aibu.InternshipAutomation.Data.Context;
using Aibu.InternshipAutomation.Data.Entities;

namespace Aibu.InternshipAutomation.Data.Dal
{
    public class DepartmentDal : IDepartmentDal
    {
        private readonly DatabaseContext _context;

        public DepartmentDal(DatabaseContext context)
        {
            _context = context;
        }

        public List<Departments> GetAll()
        {
            return _context.Department.ToList();
        }

        public string DepartmentName(int id)
        {
            return _context.Department.FirstOrDefault(p => p.Id == id).Name;
        }
    }
}