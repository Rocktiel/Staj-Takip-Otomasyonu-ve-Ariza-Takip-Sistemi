using Aibu.InternshipAutomation.Data.Base;
using Aibu.InternshipAutomation.Data.Context;
using Aibu.InternshipAutomation.Data.Entities;
using Aibu.InternshipAutomation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;

namespace Aibu.InternshipAutomation.Data.Dal
{
    public class AdminDal : IAdminDal
    {
        private readonly DatabaseContext _databaseContext;

        public AdminDal(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        public List<CompanyInfoViews> GetAll()
        {
            return _databaseContext.CompanyInfoView.ToList();
        }

        public CompanyUserss GetAllInfo(string companyName)
        {
            var company = _databaseContext.CompanyUsers.FirstOrDefault(x => x.CompanyName == companyName);
            if (company is null)
                throw new InvalidOperationException("Company bulunamadi");
            return company;
        }

        public AuthorizedPersons? UpdateAuthorizedPerson(int roleId, string name, string surname, string email , int deparmentId)
        {
            var existingAuthorizedPersons = _databaseContext.AuthorizedDepartments.FirstOrDefault(p => p.RoleId == roleId && p.DepartmentId == deparmentId);
            var existingAuthorizedPersons2 = _databaseContext.AuthorizedPerson.Find(existingAuthorizedPersons.AuthorizedPersonId);

            existingAuthorizedPersons2.Name = name;
            existingAuthorizedPersons2.Surname = surname;
            existingAuthorizedPersons2.Email = email;
            

            _databaseContext.AuthorizedPerson.Update(existingAuthorizedPersons2);
            _databaseContext.SaveChanges();

            return existingAuthorizedPersons2;
        }

        public void UploadInfoAuthorized(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                throw new ArgumentException("Yüklenen dosya boş veya geçersiz.");

            try
            {
                using (var package = new ExcelPackage(excelFile.OpenReadStream()))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    _databaseContext.AuthorizedUbys.RemoveRange(_databaseContext.AuthorizedUbys);
                    _databaseContext.SaveChanges();

                    for (int row = 4; row <= rowCount; row++) // veri 4. satırdan başlıyor
                    {
                        string nameSurname = worksheet.Cells[row, 3].Value?.ToString()?.Trim(); // "ABONE ADI SOYADI"
                        string email = worksheet.Cells[row, 10].Value?.ToString()?.Trim();       // "E-POSTA"

                        if (string.IsNullOrWhiteSpace(nameSurname) || string.IsNullOrWhiteSpace(email))
                        {
                            Console.WriteLine($"[Atlandı] Satır {row}: Eksik bilgi");
                            continue;
                        }

                        var (ad, soyad) = SplitLastWord(nameSurname);

                        if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad))
                        {
                            Console.WriteLine($"[Atlandı] Satır {row}: Ad-soyad ayrıştırılamadı -> {nameSurname}");
                            continue;
                        }

                        var person = new AuthorizedUbyss
                        {
                            Name = ad,
                            Surname = soyad,
                            Email = email
                        };

                        _databaseContext.AuthorizedUbys.Add(person);
                        Console.WriteLine($"[Eklendi] Satır {row}: {ad} {soyad} - {email}");
                    }

                    _databaseContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata Detayı: " + ex.ToString());
                throw new InvalidOperationException($"Excel dosyası işlenirken hata oluştu: {ex.Message}", ex);
            }
        }



        //Exceller .xlsx formatında olması gereliyor 
        //Excel farklı gelebilir rowları kontrol edip ona göre kodu güncelleyebilirsiniz.
        public void UploadInfoStudent(IFormFile excelFile)
        {
            if (excelFile != null && excelFile.Length > 0)
            {
                try
                {
                    using (var package = new ExcelPackage(excelFile.OpenReadStream()))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        _databaseContext.Ubys.RemoveRange(_databaseContext.Ubys);
                        _databaseContext.SaveChanges();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string studentNo = worksheet.Cells[row, 1].Value?.ToString(); // Number
                            string name = worksheet.Cells[row, 3].Value?.ToString();      // Name
                            string surname = worksheet.Cells[row, 4].Value?.ToString();   // Surname
                            string className = worksheet.Cells[row, 7].Value?.ToString(); // Class
                            string departmentIdStr = worksheet.Cells[row, 2].Value?.ToString();


                            var department = _databaseContext.Department.FirstOrDefault(p => p.Name == departmentIdStr);
                            if (department == null)
                            {
                                throw new InvalidOperationException($"Department bulunamadı: {departmentIdStr}");
                            }
                            var departmentId = department.Id;

                            var student = new Ubyss
                            {
                                Number = studentNo,
                                Name = name,
                                Surname = surname,
                                Class = className,
                                DepartmentId = departmentId // ID ile eşleştir
                            };

                            _databaseContext.Ubys.Add(student);
                        }

                        _databaseContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata Detayı: " + ex.ToString()); // Tüm stack trace ve inner hatayı gösterir
                    throw new InvalidOperationException($"Excel dosyası işlenirken hata oluştu: {ex.Message}", ex);
                }

            }
        }


        private (string Name, string Surname) SplitLastWord(string fullName)
        {
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return (null, null);

            string surname = parts[^1];
            string name = string.Join(" ", parts.Take(parts.Length - 1));

            return (name, surname);
        }

    }
}
