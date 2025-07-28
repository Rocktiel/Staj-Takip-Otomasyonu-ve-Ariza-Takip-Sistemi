using Aibu.InternshipAutomation.Data.Entities;

namespace Aibu.InternshipAutomation.Data.Base
{
    public interface IDepartmentDal
    {
        List<Departments> GetAll();
        public string DepartmentName(int id);
    }
}